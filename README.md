<div align="center">

# RianFriends

**외국인 AI 친구와 대화하고, 아바타를 꾸미고, 기상 알람을 받을 수 있는 감성 AI 친구 앱.**

[![Build](https://img.shields.io/github/actions/workflow/status/daeha76/RianFriends/ci.yml?style=flat-square&logo=github)](https://github.com/daeha76/RianFriends/actions)
[![License](https://img.shields.io/github/license/daeha76/RianFriends?style=flat-square)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512bd4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)

> 이 앱은 딸 리안(Rian)의 이름을 담아 만든 AI 친구 앱입니다. 💕

</div>

---

## Why RianFriends?

- **계층적 메모리** — 7일/30일/3개월/6개월/1년/10년 단위 대화 기억 시스템
- **Code-Switching 파싱** — 한국어 + 외국어 혼용 발화를 [원문, 병음, 뜻]으로 자동 분석
- **감성 AI 친구** — 성격·말투·관심사가 다른 외국인 AI 친구와 자연스러운 대화
- **공감 모드** — 언어 교정 없이 감정을 들어주는 Empathy 모드
- **아바타 시스템** — 시간이 지나면 배고파지는 아바타에게 간식 제공
- **기상 알람** — 매일 아침 AI 친구가 직접 깨워주는 Wake-up 알람
- **Clean Architecture** — Domain / Application / Infrastructure / API 계층 분리
- **MAUI Blazor Hybrid** — iOS / Android 단일 코드베이스

---

## Quick Start

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

`src/RianFriends.Api/appsettings.Development.json` 파일을 수정합니다:

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
  },
  "Llm": {
    "Provider": "Anthropic",
    "ConversationModel": "claude-sonnet-4-6",
    "BatchModel": "claude-haiku-4-5-20251001"
  }
}
```

### 4. DB 마이그레이션 적용

```bash
dotnet ef database update \
  --project src/RianFriends.Infrastructure \
  --startup-project src/RianFriends.Api
```

### 5. API 서버 실행

```bash
dotnet run --project src/RianFriends.Api
```

Swagger UI: `http://localhost:5000/swagger`

---

## 기술 스택

| 레이어 | 기술 |
|:-------|:-----|
| Backend API | ASP.NET Core 10 (Clean Architecture, CQRS) |
| Mobile App | .NET MAUI 10 + Blazor Hybrid |
| 인증 | Supabase Auth (Google / Apple / Kakao / 이메일) |
| DB | PostgreSQL (via Supabase) + EF Core 10 |
| Cache | Redis (Azure Cache for Redis) |
| LLM | Anthropic Claude (대화 응답 / 배치 요약) |
| 배포 | Azure App Service |

---

## 프로젝트 구조

```
RianFriends/
├── src/
│   ├── RianFriends.Domain/         # 엔티티, 도메인 이벤트, 값 객체
│   ├── RianFriends.Application/    # Command/Query Handlers, Interfaces
│   ├── RianFriends.Infrastructure/ # EF Core, Supabase Auth, Redis, LLM
│   └── RianFriends.Api/            # ASP.NET Core Web API
└── tests/
    ├── RianFriends.Domain.Tests/
    ├── RianFriends.Application.Tests/
    ├── RianFriends.Architecture.Tests/
    ├── RianFriends.Infrastructure.Tests/
    └── RianFriends.Api.Tests/
```

---

## 도메인 구조

```
User (사용자)
  └── Friend (AI 친구)
        ├── Avatar              # 아바타 외형/상태 (배고픔, 기분)
        ├── Memory[]            # 계층적 대화 메모리 (7일 ~ 10년)
        ├── Conversation[]      # 대화 세션 + 메시지 이력
        └── Notification[]      # 기상 알람 설정
```

| 도메인 | 책임 |
|:-------|:-----|
| `Identity` | 사용자 계정, 소셜 인증 |
| `Friend` | AI 친구 생성/설정, 성격 관리 |
| `Memory` | 대화 메모리 계층 관리, 요약 생성 |
| `Conversation` | 대화 세션, 메시지 저장, LLM 호출 |
| `Learning` | Code-Switching 파싱, 언어 레벨 평가 |
| `Avatar` | 아바타 상태, 간식, 외형 커스터마이징 |
| `Notification` | 기상 알람, 푸시 알림 |
| `Billing` | 플랜 관리, 토큰 할당량 |

---

## API 엔드포인트

| 메서드 | 경로 | 설명 | 인증 |
|:-------|:-----|:-----|:-----|
| POST | `/api/v1/auth/login` | 소셜/이메일 로그인 | ❌ |
| POST | `/api/v1/auth/refresh` | AccessToken 갱신 | ❌ |
| GET | `/api/v1/users/me` | 내 정보 조회 | ✅ |
| DELETE | `/api/v1/users/me` | 계정 탈퇴 | ✅ |
| GET | `/api/v1/friends` | 내 AI 친구 목록 | ✅ |
| POST | `/api/v1/friends` | AI 친구 생성 | ✅ |
| POST | `/api/v1/conversations` | 대화 세션 시작 | ✅ |
| POST | `/api/v1/conversations/{id}/messages` | 메시지 전송 | ✅ |
| GET | `/api/v1/avatars/{friendId}` | 아바타 상태 조회 | ✅ |
| POST | `/api/v1/avatars/{friendId}/feed` | 간식 주기 | ✅ |
| GET | `/health` | 서버 상태 확인 | ❌ |

---

## 테스트 실행

```bash
# 전체 테스트
dotnet test

# 카테고리별 실행
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Architecture"
dotnet test --filter "Category=Integration"

# 커버리지 측정
dotnet test --collect:"XPlat Code Coverage"
```

---

## Contributing

기여를 환영합니다! 이슈나 풀 리퀘스트를 자유롭게 제출해 주세요.

1. 이 저장소를 Fork합니다
2. 피처 브랜치를 생성합니다 (`git checkout -b feature/amazing-feature`)
3. 변경 사항을 커밋합니다 (`git commit -m 'feat: Add amazing feature'`)
4. 브랜치에 Push합니다 (`git push origin feature/amazing-feature`)
5. Pull Request를 생성합니다

---

## License

Private — All Rights Reserved.

---

<div align="center">

Made with ❤️ for Rian

</div>
