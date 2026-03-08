# Domain: Billing & LLM

> RianFriends 도메인 개요 → `domain-rianfriends.md`

---

## 소셜 인증 프로바이더

| 프로바이더 | 코드 상수 | 비고 |
|:----------|:---------|:-----|
| Google | `"google"` | 글로벌 기본 |
| Naver | `"naver"` | 한국 사용자 주력 |
| Kakao | `"kakao"` | 한국 사용자 주력 |
| Apple | `"apple"` | iOS 필수 (App Store 정책) |

- Supabase Auth OAuth 연동으로 구현
- 신규 프로바이더 추가 시 `Identity` 도메인 수정만으로 완결 (다른 도메인 영향 없음)

---

## Billing 도메인

```csharp
// Domain/Billing/UserQuota.cs
public class UserQuota
{
    public Guid UserId { get; private set; }
    public PlanType Plan { get; private set; }
    public int DailyTokenLimit { get; private set; }   // 일 허용 토큰
    public int UsedTokensToday { get; private set; }
    public DateTimeOffset QuotaResetAt { get; private set; } // 매일 자정 초기화

    public Result ConsumeTokens(int tokens)
    {
        if (UsedTokensToday + tokens > DailyTokenLimit)
            return Result.Failure("일일 토큰 한도를 초과했습니다.");
        UsedTokensToday += tokens;
        AddDomainEvent(new TokensConsumedEvent(UserId, tokens));
        return Result.Success();
    }
}

public enum PlanType { Free, Basic, Plus, Pro }
```

| 플랜 | 일 토큰 | 친구 수 |
|:-----|:-------|:-------|
| Free | 3,000 | 1 |
| Basic | 20,000 | 3 |
| Plus | 100,000 | 5 |
| Pro | 무제한 | 무제한 |

**규칙**:
- 토큰 소비는 LLM 응답 수신 후 실제 사용량 기준으로 기록 (예측값 선차감 금지)
- 한도 초과 시 대화 응답 차단 — 업그레이드 유도 메시지 반환
- 요약 배치 잡의 토큰은 운영 계정에서 차감 (사용자 할당량 소모 금지)

---

## LLM API 선택 기준

> 비유: 친구의 "두뇌" 역할. 감성적 대화를 자연스럽게 이끌면서
> 위험한 발언을 스스로 거부할 수 있는 모델이 필요하다.

> ⚠️ **모델명은 빠르게 구식이 된다.** 이 파일에 특정 버전을 박지 말 것.
> 모델명은 `appsettings.json` + Azure Key Vault에서 관리하고, 코드에 하드코딩 금지.

### 역할별 모델 티어 분리 (비용 최적화 원칙)

| 역할 | 티어 | 선택 기준 |
|:-----|:-----|:---------|
| **실시간 대화 응답** | 최상위 (Flagship) | 감성 표현, 긴 컨텍스트, 강력한 내장 안전성 |
| **Code-Switching 파싱** | 경량 (Fast) | 응답 < 1초, 저렴, 구조화 출력에 충분 |
| **메모리 요약 (배치)** | 경량 (Fast) | 비실시간이므로 비용 우선 |
| **언어 레벨 평가 (배치)** | 경량 (Fast) | 단순 분류, 가장 저렴한 옵션 |

### 모델 선택 시 평가 기준 (버전 독립)

| 평가 항목 | 왜 중요한가 |
|:---------|:-----------|
| **감성 대화 자연스러움** | 앱의 핵심 가치 — 친구처럼 느껴져야 함 |
| **내장 안전성 필터** | 욕설/성적/범죄 콘텐츠 자체 거부 능력 |
| **한국어 품질** | 주요 타겟 사용자 언어 |
| **지원 외국어 품질** | 중국어, 일본어, 영어 (학습 대상 언어) |
| **컨텍스트 길이** | 장기 대화 메모리 주입에 충분한가 |
| **가격/성능 비율** | 대화 응답용 vs 배치 처리용 분리 타당성 |
| **응답 레이턴시** | 실시간 대화 체감 (목표: < 2초) |
| **스트리밍 지원** | 타이핑 효과 구현에 필요 |

### 구현 원칙 (모델명 설정 분리)

```csharp
// Infrastructure/Llm/LlmService.cs
// 모델명은 코드에 하드코딩 금지 — appsettings에서 주입
public class LlmService : ILlmService
{
    private readonly string _conversationModel;  // 설정에서 주입
    private readonly string _batchModel;         // 설정에서 주입

    public LlmService(IConfiguration config)
    {
        _conversationModel = config["Llm:ConversationModel"]
            ?? throw new InvalidOperationException("Llm:ConversationModel 설정 필요");
        _batchModel = config["Llm:BatchModel"]
            ?? throw new InvalidOperationException("Llm:BatchModel 설정 필요");
    }
}
```

```json
// appsettings.json (버전 교체 시 이 파일만 수정)
{
  "Llm": {
    "Provider": "Anthropic",
    "ConversationModel": "claude-sonnet-4-6",
    "BatchModel": "claude-haiku-4-5-20251001"
  }
}
```

```bash
# 운영 환경 — Azure Key Vault에서 모델명 관리 (보안 + 중앙 관리)
# Llm--ConversationModel, Llm--BatchModel
```

### ILlmService 인터페이스 (Provider 교체 대응)

```csharp
// Application/Interfaces/ILlmService.cs
// 이 인터페이스는 변하지 않음 — Provider(Claude/GPT/Gemini)가 바뀌어도 유지
public interface ILlmService
{
    Task<string> GenerateResponseAsync(
        string systemPrompt, Message[] contextMessages,
        FriendMemory[] memories, CancellationToken ct = default);

    Task<string> SummarizeMemoryAsync(
        Message[] messages, MemoryLayer targetLayer, CancellationToken ct = default);

    IAsyncEnumerable<string> StreamResponseAsync(
        string systemPrompt, Message[] contextMessages,
        FriendMemory[] memories, CancellationToken ct = default);
}

// Provider별 구현체: Infrastructure/Llm/
// ├── ClaudeLlmService.cs   (현재)
// ├── OpenAiLlmService.cs   (GPT 전환 시)
// └── GeminiLlmService.cs   (Gemini 전환 시)
// MauiProgram.cs/DI에서 Provider 설정값에 따라 바인딩
```

### 향후 고려 (음성 지원 시)

```
TTS (텍스트 → 음성): ElevenLabs (감성적, 캐릭터 목소리), Azure TTS (저렴)
STT (음성 → 텍스트): Azure Speech Service, OpenAI Whisper API
```
