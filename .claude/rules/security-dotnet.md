# Security: .NET 10 + Supabase Auth (B2C Mobile)

## 인증 흐름: MAUI → Bearer Token

MAUI 앱은 브라우저 Cookie를 신뢰할 수 없음. **Bearer Token + SecureStorage** 방식 사용:

```
① MAUI 앱 → .NET API POST /auth/login (이메일/소셜)
② .NET API → Supabase Auth 인증 요청
③ Supabase JWT(AccessToken + RefreshToken) 발급
④ .NET API → 클라이언트에 토큰 반환
⑤ MAUI → SecureStorage.SetAsync("access-token", accessToken)
⑥ 이후 모든 API 요청: Authorization: Bearer {accessToken} 헤더 자동 첨부
⑦ AccessToken 만료 시 RefreshToken으로 자동 갱신
```

```csharp
// Infrastructure/Auth/AuthService.cs
public async Task<AuthResult> LoginAsync(LoginRequest req, CancellationToken ct)
{
    var session = await _supabase.Auth.SignIn(req.Email, req.Password);
    return new AuthResult(session.AccessToken, session.RefreshToken,
        session.ExpiresAt());
}

// MauiProgram.cs — DelegatingHandler로 토큰 자동 첨부
builder.Services.AddTransient<AuthTokenHandler>();
builder.Services.AddHttpClient("ApiClient", ...)
    .AddHttpMessageHandler<AuthTokenHandler>();

public class AuthTokenHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var token = await SecureStorage.GetAsync("access-token");
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, ct);
    }
}
```

### 토큰 갱신 (자동)

```csharp
// AccessToken 만료 감지 → RefreshToken으로 자동 갱신
public class AuthTokenHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(...)
    {
        var response = await base.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var refreshed = await _authService.RefreshAsync(ct);
            if (refreshed)
            {
                // 토큰 갱신 성공 → 재시도
                var token = await SecureStorage.GetAsync("access-token");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                response = await base.SendAsync(request, ct);
            }
        }
        return response;
    }
}
```

## 소셜 로그인 보안 규칙

```csharp
// .NET API — 소셜 토큰 검증 후 자체 JWT 발급
[HttpPost("auth/social")]
[AllowAnonymous]
public async Task<IActionResult> SocialLogin([FromBody] SocialLoginRequest req)
{
    // Supabase Auth가 소셜 토큰 검증 담당
    var session = await _supabase.Auth.SignInWithOAuth(req.Provider, req.AccessToken);
    return Ok(new { session.AccessToken, session.RefreshToken });
}
```

**Apple Sign In 필수 처리**:
- iOS 앱에서 소셜 로그인 제공 시 **Apple Sign In 반드시 포함** (App Store 정책)
- Apple은 이메일 숨기기 기능 제공 → `is_email_hidden` 플래그 저장 후 알림 발송 우회

## .NET API 인증 설정

```csharp
// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{supabaseProjectRef}.supabase.co/auth/v1";
        options.Audience = "authenticated";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ClockSkew = TimeSpan.FromMinutes(1)  // 시계 오차 허용
        };
    });
```

## 사용자 역할 (B2C 단순 구조)

RianFriends는 B2C 앱. Tenant/Department 없음:

| Role | 권한 |
|:-----|:-----|
| `user` | 본인 데이터만 (기본) |
| `admin` | 전체 데이터 조회, 시스템 설정 |

```csharp
// 현재 사용자 컨텍스트
public interface ICurrentUserService
{
    Guid UserId { get; }
    string Role { get; }       // "user" | "admin"
    bool IsAdmin { get; }
}

// 모든 Handler에서 UserId 기반 필터 — 다른 사용자 데이터 접근 차단
public async Task<Result<ConversationDto>> Handle(GetConversationQuery query, CancellationToken ct)
{
    var conv = await _db.Conversations
        .Where(c => c.Id == query.ConversationId && c.UserId == _currentUser.UserId) // 필수
        .FirstOrDefaultAsync(ct);
    ...
}
```

## Secret 관리

```csharp
// NEVER: appsettings.json에 시크릿 직접 기입
// CORRECT:
// - Development: User Secrets
// - Production: Azure Key Vault

if (!builder.Environment.IsDevelopment())
{
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{keyVaultName}.vault.azure.net/"),
        new DefaultAzureCredential());
}
```

## Rate Limiting (API 남용 / LLM 비용 폭발 방지)

```csharp
// Program.cs — 사용자별 + 전역 Rate Limit
builder.Services.AddRateLimiter(options =>
{
    // 대화 API: 사용자당 분당 10회
    options.AddPolicy("ConversationPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous",
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6
            }));

    // 전역: IP당 분당 60회
    options.AddPolicy("GlobalPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// Controller
[RateLimiter(policyName: "ConversationPolicy")]
[HttpPost("conversations/{id}/messages")]
public async Task<IActionResult> SendMessage(...) { ... }
```

## HTTPS 강제

```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}
```

## 입력 검증 (FluentValidation + MediatR Pipeline)

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var failures = _validators
            .Select(v => v.Validate(request))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
            throw new ValidationException(failures);

        return await next();
    }
}
```

### ValidationException → Problem Details

```csharp
builder.Services.AddProblemDetails();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var (status, title, errors) = exception switch
        {
            ValidationException ve => (400, "Validation Failed",
                ve.Errors.GroupBy(e => e.PropertyName)
                         .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),
            _ => (500, "Internal Server Error", (Dictionary<string, string[]>?)null)
        };
        context.Response.StatusCode = status;
        await Results.Problem(title: title, statusCode: status,
            extensions: errors is not null
                ? new Dictionary<string, object?> { ["errors"] = errors }
                : null
        ).ExecuteAsync(context);
    });
});
```

## 보안 체크리스트 (커밋 전 필수)

- [ ] `appsettings.json`에 시크릿 없음 (API Key, Password, Token, LLM Key)
- [ ] 모든 API 엔드포인트에 `[Authorize]` 또는 `[AllowAnonymous]` 명시
- [ ] 모든 Query/Command Handler에서 `UserId` 필터 적용 (타 사용자 데이터 접근 차단)
- [ ] SQL은 EF Core 파라미터화 쿼리만 사용 (Raw string SQL 금지)
- [ ] 에러 응답에 스택 트레이스 미포함 (Production 환경)
- [ ] Rate Limiting 설정 확인 (LLM 호출 엔드포인트 필수)
- [ ] CORS Origins가 wildcard `*` 아님
- [ ] `ValidationException` → Problem Details 변환 미들웨어 등록 확인
- [ ] SecureStorage 사용 확인 — 토큰을 Preferences나 파일에 저장 금지
- [ ] LLM API Key는 절대 모바일 앱 번들에 포함 금지 — 서버에서만 호출
