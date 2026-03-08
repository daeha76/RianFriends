# 인증 하이브리드 전환 계획

> Supabase Auth는 소셜 토큰 검증만, .NET API가 자체 JWT 발급

## 현재 → 변경 후 흐름

### AS-IS
```
MAUI → .NET API → Supabase Auth → Supabase JWT 반환 → MAUI 저장
이후 요청: Authorization: Bearer {Supabase JWT}
Program.cs: Supabase Authority로 JWT 검증
```

### TO-BE
```
MAUI → .NET API → Supabase Auth (소셜 토큰 검증만, 유저 정보 획득)
                → .NET API가 자체 JWT 발급 → MAUI SecureStorage 저장
이후 요청: Authorization: Bearer {자체 JWT}
Program.cs: 자체 Signing Key로 JWT 검증
RefreshToken: DB에 저장, 만료/회전(rotation) 관리
```

## 변경 파일 목록

### 1. 새 파일 생성

| 파일 | 용도 |
|:-----|:-----|
| `Application/Identity/Interfaces/IJwtTokenService.cs` | 자체 JWT 생성/검증 인터페이스 |
| `Infrastructure/Identity/JwtTokenService.cs` | JWT 발급 구현체 (HS256, 설정에서 키 주입) |
| `Domain/Identity/RefreshToken.cs` | RefreshToken 엔티티 (DB 저장) |
| `Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs` | EF Core 설정 |

### 2. 수정 파일

| 파일 | 변경 내용 |
|:-----|:---------|
| `Program.cs` | JWT 인증을 Supabase Authority → 자체 Signing Key 검증으로 변경 |
| `IAuthService.cs` | 반환 타입을 `AuthResultDto` → `SocialUserInfo`로 변경 (JWT 발급 책임 분리) |
| `SupabaseAuthService.cs` | Supabase 토큰을 그대로 반환하지 않고, 사용자 정보만 반환 |
| `LoginCommandHandler.cs` | IAuthService로 검증 → IJwtTokenService로 자체 JWT 발급 |
| `RefreshTokenCommandHandler.cs` | Supabase refresh → DB RefreshToken 조회 + 자체 JWT 재발급 |
| `AuthResultDto.cs` | 유지 (AccessToken, RefreshToken 필드는 동일) |
| `InfrastructureServiceExtensions.cs` | IJwtTokenService DI 등록 |
| `AppDbContext.cs` | RefreshToken DbSet 추가 |

### 3. 설정 추가

```json
// appsettings.json
{
  "Jwt": {
    "Secret": "(User Secrets / Key Vault)",
    "Issuer": "rianfriends-api",
    "Audience": "rianfriends-app",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 30
  }
}
```

## 구현 순서

1. **Domain**: `RefreshToken` 엔티티 생성
2. **Application**: `IJwtTokenService` 인터페이스 + `SocialUserInfo` DTO 생성
3. **Application**: `IAuthService` 수정 (반환 타입 변경)
4. **Infrastructure**: `JwtTokenService` 구현 + `RefreshTokenConfiguration` + `AppDbContext` 수정
5. **Infrastructure**: `SupabaseAuthService` 수정 (사용자 정보만 반환)
6. **Application**: `LoginCommandHandler`, `RefreshTokenCommandHandler` 수정
7. **Api**: `Program.cs` JWT 설정 변경
8. **Tests**: 기존 Login/Register 테스트 업데이트

## 주의사항

- RefreshToken rotation: 사용 시 이전 토큰 무효화 (보안)
- JWT Secret은 최소 256bit (32자 이상)
