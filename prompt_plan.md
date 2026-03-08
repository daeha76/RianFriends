# RianFriends 구현 계획

> 승인일: 2026-03-08
> 총 예상 기간: 18주

## 전체 Phase 요약

| Phase | 기간 | 내용 |
|:------|:-----|:-----|
| 0 | 1주 | 솔루션 초기화, CI 파이프라인 |
| 1 | 3주 | 소셜 로그인 (Identity + 기반 인프라) |
| 2 | 6주 | AI 친구 대화 (Friend + Memory + Conversation + Learning) |
| 3 | 3주 | 아바타 + 기상 알람 (Avatar + Notification) |
| 4 | 3주 | 인앱 결제 + 앱스토어 제출 준비 (Billing) |
| 5 | 2주 | Azure 배포 + 운영 모니터링 |

---

## Phase 0: 개발 환경 구성 및 솔루션 초기화 (1주)

### 환경 준비
- .NET 10 SDK 설치 확인
- MAUI Workload 설치: `dotnet workload install maui`
- iOS/Android 에뮬레이터 설정
- Redis 로컬 설치 (Docker: `redis:7-alpine`)

### 솔루션 초기화

```bash
dotnet new sln --format slnx -n RianFriends
```

생성 순서:
1. `RianFriends.Domain` (classlib, net10.0)
2. `RianFriends.Application` (classlib, net10.0)
3. `RianFriends.Infrastructure` (classlib, net10.0)
4. `RianFriends.Api` (webapi, net10.0)
5. `RianFriends.App` (maui-blazor, net10.0)
6. `RianFriends.Contracts` (classlib, net10.0)
7. 5개 테스트 프로젝트 (xunit)

### 핵심 NuGet 패키지

| 프로젝트 | 패키지 |
|:---------|:-------|
| Domain | (없음, 순수 C#) |
| Application | MediatR, FluentValidation, FluentValidation.DependencyInjectionExtensions |
| Infrastructure | Microsoft.EntityFrameworkCore, Npgsql.EntityFrameworkCore.PostgreSQL, EFCore.NamingConventions, StackExchange.Redis, Microsoft.Extensions.Caching.StackExchangeRedis, Supabase |
| Api | Serilog.AspNetCore, Serilog.Sinks.ApplicationInsights, Serilog.Sinks.Http, Swashbuckle.AspNetCore, Microsoft.AspNetCore.Authentication.JwtBearer, Azure.Identity, Azure.Security.KeyVault.Secrets |
| App (MAUI) | Microsoft.AspNetCore.Components.WebView.Maui |
| 테스트 | xunit, Moq, FluentAssertions, Testcontainers.PostgreSql, NetArchTest.Rules, Microsoft.AspNetCore.Mvc.Testing |

### 공통 설정 파일
- `Directory.Build.props` — Nullable enable, TreatWarningsAsErrors, LangVersion 13
- `Directory.Packages.props` — 중앙 패키지 버전 관리 (CPM)
- `.gitignore`
- `.editorconfig`

### Phase 0 산출물
- [ ] 빌드 성공하는 빈 솔루션 (`dotnet build` 0 errors)
- [ ] Architecture.Tests 레이어 의존성 테스트 통과
- [ ] GitHub Actions CI 파이프라인 (build + test)
- [ ] README.md (개발 환경 셋업 가이드)

---

## Phase 1: Identity + 기반 인프라 (3주)

### 도메인: Identity
- `User`, `UserProfile` 엔티티
- Commands: RegisterUser, SocialLogin, RefreshToken, DeleteUserAccount
- Queries: GetCurrentUser
- 소셜 프로바이더: Google → Apple → Kakao

### 기반 인프라
- AppDbContext (UseNpgsql + UseSnakeCaseNamingConvention)
- BaseEntity, AuditableEntity, Result<T>, DomainException
- MediatR 파이프라인: ValidationBehavior, LoggingBehavior
- JWT Bearer 인증, Problem Details, Rate Limiting, Health Check, Swagger

### DB 스키마: users 테이블

```sql
users
  id UUID PK, email TEXT UNIQUE, nickname TEXT, birth_date DATE,
  country_code TEXT, plan_type TEXT DEFAULT 'free', role TEXT DEFAULT 'user',
  is_email_hidden BOOLEAN DEFAULT false, created_at TIMESTAMPTZ, updated_at TIMESTAMPTZ, deleted_at TIMESTAMPTZ
```

### Phase 1 산출물
- [ ] 소셜 로그인 → 토큰 발급 → API 호출 흐름 동작
- [ ] 첫 번째 EF Core Migration 적용

---

## Phase 2: Friend + Memory + Conversation (6주)

### 도메인: Friend
- `Friend`, `FriendPersona` 엔티티
- DB: friends 테이블 (personality_type, interests TEXT[], speech_style)

### 도메인: Memory
- `FriendMemory` 엔티티, MemoryLayer enum
- MemorySummaryJob (배치, IHostedService)
- Redis: `conversation:{id}:context` TTL 30분

### 도메인: Conversation
- `ConversationSession`, `Message`, `EmpathySettings`
- SendMessage 흐름: 필터 → 컨텍스트 → SystemPrompt → LLM 스트리밍 → 저장 → 이벤트
- SSE 스트리밍 필수

### 도메인: Learning
- Code-Switching 파싱 (비동기 후처리)
- 언어 레벨 평가 (10회마다 배치)

### Phase 2 산출물
- [ ] AI 친구와 실제 대화 가능 (스트리밍 포함)
- [ ] Code-Switching 파싱 결과 채팅 화면 표시
- [ ] 공감 모드 전환 동작
- [ ] Memory 배치 잡 동작

---

## Phase 3: Avatar + Notification (3주)

### 도메인: Avatar
- `Avatar`, `Snack` 엔티티
- HungerIncreaseJob (1시간마다 +5)
- Redis: `avatar:{friendId}:state` TTL 1시간

### 도메인: Notification
- `WakeUpAlarm`, `DeviceToken` 엔티티
- 기상 알람: MAUI 로컬 알림 (MAUI Community Toolkit)
- 배고픔 알림: FCM/APNs

### Phase 3 산출물
- [ ] 배고픔 시스템 동작
- [ ] 기상 알람 로컬 알림 동작 (iOS + Android)
- [ ] 배고픔 푸시 알림 발송

---

## Phase 4: Billing + 완성도 (3주)

### 도메인: Billing
- `UserQuota`, `Subscription` 엔티티
- RevenueCat Webhook: POST /api/v1/webhooks/revenuecat
- 플랜: Free(3K토큰/1친구) / Basic(20K/3) / Plus(100K/5) / Pro(무제한)

### 완성도 작업
- MessageInputFilter (3단계: Crisis/Blocked/Warned)
- 강제 업데이트 체크
- 보안 체크리스트 전항목 통과

### Phase 4 산출물
- [ ] RevenueCat 인앱 결제 동작
- [ ] 플랜별 토큰 한도 적용
- [ ] 앱스토어 제출 요건 충족

---

## Phase 5: 배포 + 운영 준비 (2주)

### Azure 리소스
- App Service (Linux, B2), Redis Cache (C1), Key Vault, Application Insights, Notification Hubs, Blob Storage

### CI/CD
- GitHub Actions: build+test → staging → production
- iOS/Android 앱 빌드 별도 workflow

### Phase 5 산출물
- [ ] Staging + Production 배포
- [ ] iOS TestFlight + Android 내부 테스트 배포
- [ ] 운영 모니터링 대시보드

---

## 전체 DB 테이블 목록

users, friends, friend_memories, conversation_sessions, messages,
language_levels, word_book_entries, avatars, snacks,
wake_up_alarms, device_tokens, user_quotas, subscriptions, audit_logs

---

## 주요 리스크

| 수준 | 리스크 | 대응 |
|:-----|:-------|:-----|
| HIGH | iOS 빌드 macOS 필수 | Phase 0에서 CI 환경 확인 |
| HIGH | Apple Sign In 필수 | Phase 1 최우선 구현 |
| HIGH | RevenueCat IAP 심사 | 외부 결제 링크 금지 |
| MEDIUM | LLM 응답 지연 | SSE 스트리밍 필수 |
| MEDIUM | 배고픔 잡 중복 실행 | Hangfire 또는 DB 잠금 |
| MEDIUM | Code-Switching 파싱 비용 | 정규식 사전 필터링 |
