# Domain: Memory & Learning

> RianFriends 도메인 개요 → `domain-rianfriends.md`

---

## Memory First: 계층적 메모리 전략

모든 대화 처리는 아래 계층을 준수한다:

```
MemoryLayer
├── ShortTerm   (7일)    — 최신 대화 원문 보존
├── MidTerm     (30일)   — 주제별 요약
├── Quarter     (3개월)  — 관계 패턴, 관심사
├── HalfYear    (6개월)  — 주요 사건, 감정 기록
├── Annual      (1년)    — 연간 회고 요약
└── Decade      (10년)   — 핵심 관계 정체성 요약
```

**규칙**:
- 실시간 대화 응답에는 ShortTerm + MidTerm만 컨텍스트로 주입 (토큰 절약)
- 상위 레이어 요약은 배치(Batch) 처리 — 실시간 대화에 영향 금지
- 요약 생성은 `MemorySummaryJob` (백그라운드 서비스)에서 담당
- 레이어 간 이동은 만료 시간 기준으로 자동 트리거

```csharp
// Domain/Memory/MemoryLayer.cs
public enum MemoryLayer
{
    ShortTerm,   // 7일
    MidTerm,     // 30일
    Quarter,     // 3개월
    HalfYear,    // 6개월
    Annual,      // 1년
    Decade       // 10년
}

public class FriendMemory
{
    public Guid Id { get; private set; }
    public Guid FriendId { get; private set; }
    public MemoryLayer Layer { get; private set; }
    public string Summary { get; private set; }    // LLM이 생성한 요약
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
}
```

---

## LLM 연동 원칙

```csharp
// Application/Interfaces/ILlmService.cs
public interface ILlmService
{
    Task<string> GenerateResponseAsync(
        string systemPrompt,
        Message[] contextMessages,
        FriendMemory[] memories,
        CancellationToken ct = default);

    Task<string> SummarizeMemoryAsync(
        Message[] messages,
        MemoryLayer targetLayer,
        CancellationToken ct = default);
}
```

**규칙**:
- LLM 호출은 Application 레이어 인터페이스로만 — Infrastructure에서 구현
- 토큰 절약: 컨텍스트 주입 시 ShortTerm(원문) + MidTerm 이상(요약)만 포함
- 요약 생성(배치)과 실시간 응답 생성의 LLM 호출은 반드시 분리

---

## Code-Switching Support: 혼용 언어 파싱

사용자의 혼용 언어(한국어 + 중국어, 한국어 + 영어 등)를 즉시 파싱하여 구조화된 데이터로 변환:

```csharp
// Domain/Learning/CodeSwitchSegment.cs
public record CodeSwitchSegment(
    string Original,     // 원문 ("你好")
    string Romanized,    // 병음/발음 ("nǐ hǎo")
    string Meaning,      // 뜻 ("안녕하세요")
    string Language      // "zh-CN", "en", "ko" 등
);

// Application/Learning/ParseCodeSwitchQuery.cs
// 메시지 텍스트 → CodeSwitchSegment[] 반환
// LLM 호출로 파싱 (Claude API)
public record ParseCodeSwitchQuery(string MessageText, string UserNativeLanguage)
    : IRequest<Result<CodeSwitchSegment[]>>;
```

**규칙**:
- 파싱은 메시지 저장 후 비동기로 처리 (응답 지연 금지)
- 결과는 `Message.CodeSwitchData` (JSONB)에 저장
- 지원 언어: 중국어(간체/번체), 영어, 일본어 (우선순위 순)

---

## Friend 페르소나 도메인

AI 친구는 국적/성격/관심사가 다른 독립 캐릭터. 사용자가 직접 선택하거나 랜덤 매칭:

```csharp
// Domain/Friend/FriendPersona.cs
public class FriendPersona
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }           // "Mei", "Yuki", "Emma"
    public string Nationality { get; private set; }    // "zh-CN", "ja", "en-GB"
    public string TargetLanguage { get; private set; } // 사용자가 배우는 언어 코드
    public PersonalityType Personality { get; private set; }
    public string[] Interests { get; private set; }    // ["kpop", "cooking", "gaming"]
    public SpeechStyle SpeechStyle { get; private set; }
}

public enum PersonalityType { Energetic, Quiet, Playful, Serious }
public enum SpeechStyle { Formal, Casual, EmojiHeavy, Mixed }
```

**규칙**:
- 친구 대화 응답 생성 시 `FriendPersona`의 성격/말투를 시스템 프롬프트에 반영
- 무료 티어: 친구 1명 / Basic: 3명 / Plus 이상: 5명+

---

## 언어 레벨 시스템

대화 분석으로 자동 측정 (명시적 테스트 없음):

```csharp
// Domain/Learning/LanguageLevel.cs
public enum LanguageLevel
{
    Infant,       // 단어 몇 개, 한국어 보조 최대
    Elementary,   // 짧은 문장, 칭찬/반복 위주
    Middle,       // 일상 문장, 자연스러운 교정
    High,         // 관용어·슬랭 도입
    Advanced      // 원어민 수준, 뉘앙스 중심
}
```

**규칙**:
- `Conversation` 10회마다 레벨 재평가 (백그라운드 잡)
- 레벨 상승 시 친구의 한국어 보조 비율 자동 감소
- 레벨은 언어별로 독립 저장 (중국어 Middle, 영어 Elementary 가능)
