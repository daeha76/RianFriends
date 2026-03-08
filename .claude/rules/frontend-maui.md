# Frontend: .NET 10 MAUI Blazor Hybrid

## 기술 선택: MAUI Blazor Hybrid

- **BlazorWebView** 컴포넌트로 네이티브 앱 안에 Blazor UI 삽입
- iOS / Android 동시 지원 (단일 코드베이스)
- Razor 컴포넌트는 WebView 내에서 실행 — SSR/WASM 구분 없음
- 네이티브 API(카메라, 알림, 센서)는 MAUI 플랫폼 서비스로 접근

## 절대 원칙: 직접 API 접근 금지

Blazor 컴포넌트는 **절대 Supabase·LLM API에 직접 접근하지 않습니다.**
모든 데이터는 .NET API 엔드포인트를 통해서만 접근:

```csharp
// WRONG: LLM API 직접 호출
var response = await anthropicClient.SendMessage(...); // ❌

// CORRECT: .NET API를 통해 접근
var response = await chatApiClient.SendMessageAsync(...); // ✅
```

## 프로젝트 구조

```
src/
├── RianFriends.App/             # MAUI 앱 프로젝트
│   ├── MauiProgram.cs           # 앱 진입점, DI 설정
│   ├── MainPage.xaml            # BlazorWebView 호스팅
│   ├── Platforms/               # iOS, Android 플랫폼별 코드
│   │   ├── Android/
│   │   └── iOS/
│   └── wwwroot/                 # 정적 자산 (CSS, 이미지)
├── RianFriends.UI/              # Razor Class Library (공통 컴포넌트)
│   ├── Pages/                   # 화면 단위 Razor 컴포넌트
│   └── Components/
│       ├── Avatar/
│       ├── Chat/
│       └── Shared/
└── RianFriends.Api/             # .NET Web API (백엔드)
```

## HttpClient 설정

MAUI는 브라우저가 아니므로 CORS 제약 없음. 환경별 baseAddress 처리 필요:

```csharp
// MauiProgram.cs
builder.Services.AddHttpClient("ApiClient", client =>
{
    // 에뮬레이터/시뮬레이터: 개발용 주소
    // 실제 기기 또는 운영: appsettings.json의 ApiBaseUrl 사용
    client.BaseAddress = new Uri(
        builder.Configuration["ApiBaseUrl"] ?? "http://10.0.2.2:7001");
});
```

```json
// appsettings.json (앱 번들에 포함 — 시크릿 금지)
{
  "ApiBaseUrl": "https://api.rianfriends.com"
}
```

## API 클라이언트 자동 생성 (OpenAPI 기반)

수동 HttpClient 코드 작성 금지. Api 프로젝트 Swagger 스펙에서 자동 생성:

```bash
dotnet tool install --global NSwag.ConsoleCore
nswag openapi2csclient \
  /input:https://localhost:7001/swagger/v1/swagger.json \
  /output:src/RianFriends.UI/ApiClients/GeneratedApiClient.cs
```

## JS Interop

MAUI BlazorWebView는 항상 렌더링 완료 후 JS 접근 가능 (SSR 우려 없음).
일관성을 위해 `OnAfterRenderAsync` 패턴 동일하게 유지:

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
        await JS.InvokeVoidAsync("initAvatarCanvas");
}
```

**절대 금지**: `localStorage` / `sessionStorage` JS API — MAUI에서 신뢰할 수 없음.

## 로컬 저장소: Preferences & SecureStorage

브라우저 localStorage 대신 MAUI 네이티브 저장소 사용:

```csharp
// 민감하지 않은 UI 설정 (테마, 언어, 아바타 배치 등)
Preferences.Set("theme", "dark");
var theme = Preferences.Get("theme", "light");

// 민감한 정보 (인증 토큰) — iOS Keychain / Android Keystore 자동 연동
await SecureStorage.SetAsync("auth-token", token);
var token = await SecureStorage.GetAsync("auth-token");
```

## 푸시 알림 (기상 알람, 간식 알림)

```csharp
// Application/Interfaces/IPushNotificationService.cs
public interface IPushNotificationService
{
    Task RegisterDeviceAsync();
    Task ScheduleLocalAlarmAsync(WakeUpAlarmDto alarm);   // 기상 알람
    Task CancelAlarmAsync(Guid alarmId);
    Task ShowHungerAlertAsync(string friendName);         // 배고픔 알림
}
```

- **로컬 알림**: MAUI Community Toolkit `LocalNotificationCenter` 사용
- **원격 푸시**: Firebase FCM (Android) / APNs (iOS) — .NET API에서 발송
- 기상 알람은 로컬 알림 스케줄링 우선 (서버 의존도 최소화)

```csharp
// MauiProgram.cs — 플랫폼별 구현 주입
#if ANDROID
builder.Services.AddSingleton<IPushNotificationService, AndroidPushNotificationService>();
#elif IOS
builder.Services.AddSingleton<IPushNotificationService, IosPushNotificationService>();
#endif
```

## 플랫폼별 코드 분기

```csharp
// CORRECT: DI로 플랫폼별 구현 주입 (위 예시처럼)

// WRONG: Razor 컴포넌트 안에서 직접 분기
@if (DeviceInfo.Platform == DevicePlatform.Android) { ... } // ❌
```

Razor 컴포넌트는 플랫폼을 몰라야 한다. 플랫폼 분기는 MauiProgram.cs + 인터페이스로만.

## 메모리 관리 (필수)

BlazorWebView는 네이티브 WebView 메모리 사용. 대용량 데이터 상태 저장 금지:

```csharp
// WRONG: 전체 대화 이력을 컴포넌트 상태로 보관
private List<Message> _allMessages = new(); // 수천 건 → OOM 위험

// CORRECT: 최근 N건만 유지
private List<MessageDto> _recentMessages = new(); // 최근 50건
```

`IAsyncDisposable` 반드시 구현 — SignalR, 타이머, 이벤트 구독 해제:

```csharp
@implements IAsyncDisposable

@code {
    public async ValueTask DisposeAsync()
    {
        await _hubConnection?.DisposeAsync();
    }
}
```

## 오프라인 지원

```csharp
if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
{
    // 대화: 온라인 필수 (LLM 호출)
    // 아바타 상태: 로컬 Preferences 캐시 fallback
    // 알람: 로컬 알림으로 작동 유지
}
```

## 에러 처리

```razor
<ErrorBoundary>
    <ChildContent>@Body</ChildContent>
    <ErrorContent Context="ex">
        <ErrorDisplay Exception="ex" />
    </ErrorContent>
</ErrorBoundary>
```

## 빌드 / 배포

```bash
# Android
dotnet publish -f net10.0-android -c Release

# iOS (macOS 필요)
dotnet publish -f net10.0-ios -c Release
```

- 앱 버전: `MauiManifest` (Android) / `Info.plist` (iOS)에서 관리
- 시크릿은 앱 번들에 포함 금지 — 서버에서 발급받아 `SecureStorage`에 저장
