# Domain: Avatar & Empathy

> RianFriends 도메인 개요 → `domain-rianfriends.md`

---

## Avatar 도메인

아바타는 시간이 지남에 따라 배고파지고, 간식을 주면 배고픔이 해소된다:

```csharp
// Domain/Avatar/Avatar.cs
public class Avatar
{
    public Guid Id { get; private set; }
    public Guid FriendId { get; private set; }
    public string AvatarStyle { get; private set; }   // 외형 스타일 코드
    public int HungerLevel { get; private set; }       // 0(배부름) ~ 100(매우배고픔)
    public AvatarMood Mood { get; private set; }       // 기분 상태
    public DateTimeOffset LastFedAt { get; private set; }

    public Result Feed(Snack snack)
    {
        if (HungerLevel == 0) return Result.Failure("이미 배가 불러요!");
        HungerLevel = Math.Max(0, HungerLevel - snack.SatietyValue);
        LastFedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new AvatarFedEvent(Id, snack.Id));
        return Result.Success();
    }
}

public enum AvatarMood { Happy, Neutral, Hungry, Sad, Excited }
```

**배고픔 증가 규칙**:
- 1시간마다 HungerLevel +5 (백그라운드 잡)
- HungerLevel >= 80: 아바타가 간식 요청 푸시 알림 발송
- HungerLevel == 100: 아바타 외형 변화 (풀이 죽은 상태)

**Redis 캐싱** (TTL: 1시간):
- Key: `avatar:{friendId}:state`
- Value: HungerLevel, Mood (빈번한 조회 최적화)

---

## Notification 도메인

```csharp
// Domain/Notification/WakeUpAlarm.cs
public class WakeUpAlarm
{
    public Guid Id { get; private set; }
    public Guid FriendId { get; private set; }
    public TimeOnly AlarmTime { get; private set; }    // 기상 시각
    public DayOfWeek[] ActiveDays { get; private set; } // 활성 요일
    public bool IsEnabled { get; private set; }
    public string TimeZoneId { get; private set; }      // 사용자 타임존
}
```

**규칙**:
- 타임존 관련 알람/스케줄은 **date-calculation.md 규칙** 적용 (암산 금지)
- 알람 스케줄 Redis 캐싱 (TTL: 24시간) — Key: `alarm:{userId}:schedule`

---

## Empathy 도메인 (공감 모드)

언어 학습 없이 친구가 그냥 들어주는 모드. 공감 게이지로 응답 스타일 비율 조정:

```csharp
// Domain/Conversation/ConversationMode.cs
public enum ConversationMode
{
    Language,  // 기본: 언어 학습 + 대화
    Empathy    // 공감 모드: 언어 교정 없음, 듣고 공감
}

// Domain/Conversation/EmpathySettings.cs
public class EmpathySettings
{
    public int Gauge { get; private set; }              // 0–100
    public ConversationMode Mode { get; private set; }
    public GaugeControlMode ControlMode { get; private set; }  // 제어 방식

    // 자동 감지: 부정 감정 키워드 → 공감 모드 제안 후 사용자 확인
    // 수동 오버라이드: 사용자가 슬라이더로 직접 게이지 설정
    // → 수동 오버라이드 중에는 자동 감지 제안 표시 안 함

    public void SetGauge(int value, GaugeControlMode source)
    {
        if (value < 0 || value > 100)
            throw new DomainException("게이지는 0–100 범위여야 합니다.");
        Gauge = value;
        ControlMode = source;
        AddDomainEvent(new EmpathyGaugeChangedEvent(Gauge, source));
    }

    public void ResetToPersonalityDefault(int defaultGauge)
    {
        // 세션 종료 또는 사용자가 초기화 요청 시 호출
        Gauge = defaultGauge;
        ControlMode = GaugeControlMode.Auto;
    }
}

public enum GaugeControlMode
{
    Auto,            // 자동 감지 활성 (기본)
    ManualOverride   // 사용자 수동 설정 중 — 자동 제안 억제
}
```

### 게이지별 응답 전략

| 게이지 범위 | 응답 스타일 |
|:-----------|:-----------|
| 80–100 | 공감 100% — "그랬구나... 많이 힘들었겠다" + 감정 확인 질문 |
| 50–79 | 공감 70% + 부드러운 제안 30% |
| 20–49 | 공감 30% + 구체적 해결책 70% |
| 0–19 | 논리적 해결책 위주, 공감은 한 줄 |

### 친구 성격별 기본 게이지 (Auto 모드 초기값)

| PersonalityType | 기본 Gauge |
|:---------------|:----------|
| Quiet | 70 |
| Energetic | 60 |
| Playful | 50 |
| Serious | 30 |

### 제어 방식 (Auto + ManualOverride 동시 지원)

| 상황 | 동작 |
|:-----|:-----|
| 기본 (Auto) | 친구 성격 기본값으로 시작. 부정 감정 감지 시 "오늘은 그냥 들어줄까?" 제안 |
| 사용자가 제안 수락 | `ConversationMode.Empathy` + Gauge = 성격 기본값으로 전환 |
| 사용자가 슬라이더 조작 | `GaugeControlMode.ManualOverride` — 이후 자동 제안 억제 |
| 사용자가 초기화 버튼 누름 | `ResetToPersonalityDefault()` → Auto 모드 복귀 |
| 세션 종료 | Language 모드로 복귀, 게이지 성격 기본값으로 초기화 |

**규칙**:
- 공감 모드에서는 `CodeSwitch` 파싱 결과를 응답에 **절대 포함하지 않음** (문법 지적 금지)
- `ManualOverride` 중에는 자동 감지 제안 **표시하지 않음** (사용자 설정 존중)
- 모드는 **세션(ConversationSession) 단위** 유지 — 세션 종료 시 Language 모드 자동 복귀
- 게이지는 대화 중 **실시간 슬라이더** 로 변경 가능 (즉시 다음 응답에 반영)
- 공감 게이지는 **응답 생성 시스템 프롬프트에 수치로 명시** — "현재 공감 게이지: 80, 응답의 80%는 감정 공감으로 구성"
