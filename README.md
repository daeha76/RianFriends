# RianFriends

> 외국인 AI 친구와 대화하고, 아바타를 꾸미고, 기상 알람을 받을 수 있는 감성 AI 친구 앱.

## 기술 스택

| 레이어 | 기술 |
|:-------|:-----|
| Backend API | ASP.NET Core 10 (Clean Architecture) |
| MAUI App | .NET MAUI 10 + Blazor Hybrid |
| 인증 | Supabase Auth (Google / Apple / Kakao / 이메일) |
| DB | PostgreSQL (via Supabase) + EF Core 10 |
| Cache | Redis (Azure Cache for Redis) |
| 배포 | Azure App Service |

## 개발 환경 요구사항

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (PostgreSQL + Redis 로컬 실행)
- [Visual Studio 2022 17.12+](https://visualstudio.microsoft.com/) 또는 [Rider 2024.3+](https://www.jetbrains.com/rider/)
- PostgreSQL 클라이언트 (선택사항): pgAdmin / DBeaver

## 사전 설치

```bash
# MAUI Workload 설치
dotnet workload install maui

# Wasm Tools (향후 Blazor WASM 지원 시)
dotnet workload install wasm-tools
```

## 로컬 개발 시작

### 1. 저장소 클론

```bash
git clone https://github.com/daeha76/RianFriends.git
cd RianFriends
```

### 2. 인프라 실행 (Docker)

```bash
docker run -d --name rianfriends-db \
  -e POSTGRES_DB=rianfriends \
  -e POSTGRES_USER=sa \
  -e POSTGRES_PASSWORD=localpassword \
  -p 5432:5432 postgres:17-alpine

docker run -d --name rianfriends-redis \
  -p 6379:6379 redis:7-alpine
```

### 3. 환경 설정

`src/RianFriends.Api/appsettings.Development.json` 파일을 수정하세요:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=rianfriends;Username=sa;Password=localpassword"
  },
  "Supabase": {
    "Url": "https://<your-project>.supabase.co",
    "AnonKey": "<your-anon-key>",
    "ServiceKey": "<your-service-key>",
    "ProjectRef": "<your-project-ref>"
  }
}
```

### 4. DB 마이그레이션 적용

```bash
dotnet ef database update --project src/RianFriends.Infrastructure --startup-project src/RianFriends.Api
```

### 5. API 서버 실행

```bash
dotnet run --project src/RianFriends.Api
```

Swagger UI: http://localhost:5000/swagger

## 테스트 실행

```bash
# 전체 테스트
dotnet test

# 카테고리별 실행
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Architecture"
dotnet test --filter "Category=Integration"
```

## 프로젝트 구조

```
RianFriends/
  src/
    RianFriends.Domain/         # 엔티티, 도메인 이벤트, 값 객체
    RianFriends.Application/    # Command/Query Handlers, Interfaces
    RianFriends.Infrastructure/ # EF Core, Supabase Auth, Redis
    RianFriends.Api/            # ASP.NET Core Web API
  tests/
    RianFriends.Domain.Tests/
    RianFriends.Application.Tests/
    RianFriends.Architecture.Tests/
    RianFriends.Infrastructure.Tests/
    RianFriends.Api.Tests/
```

## API 엔드포인트 (Phase 1)

| 메서드 | 경로 | 설명 | 인증 |
|:-------|:-----|:-----|:-----|
| POST | /api/v1/auth/login | 소셜/이메일 로그인 | ❌ |
| POST | /api/v1/auth/refresh | AccessToken 갱신 | ❌ |
| GET | /api/v1/users/me | 내 정보 조회 | ✅ |
| POST | /api/v1/users/me/register | 최초 프로필 등록 | ✅ |
| PUT | /api/v1/users/me | 프로필 수정 | ✅ |
| DELETE | /api/v1/users/me | 계정 탈퇴 | ✅ |
| GET | /health | 서버 상태 확인 | ❌ |

## CI/CD

GitHub Actions를 통해 `main`, `develop` 브랜치 Push/PR 시 자동 빌드 및 테스트가 실행됩니다.

- `.github/workflows/ci.yml` 참조

## 라이선스

Private — All Rights Reserved.
