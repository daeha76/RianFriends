---
name: verify-rianfriends
description: >
  RianFriends 도메인 규칙 준수 여부를 검증합니다.
  Use when: "도메인 규칙 확인해", "이 코드 규칙 맞아?", "verify domain",
  "rianfriends 규칙 검사", "도메인 검증", "아키텍처 확인".
---

# Verify RianFriends Domain

## 목적

작성한 코드가 `domain-rianfriends.md` 규칙을 준수하는지 체크한다.
파일 경로 또는 코드 스니펫을 받아 아래 체크리스트를 적용한다.

## 체크리스트

### Memory 도메인

- [ ] 실시간 대화 응답 경로에 메모리 요약 로직이 없는가?
- [ ] 요약 생성이 `MemorySummaryJob` (백그라운드) 에서만 호출되는가?
- [ ] 컨텍스트 주입 시 ShortTerm + MidTerm만 포함하는가? (Quarter 이상 직접 주입 금지)
- [ ] `FriendMemory` 필드가 `private set` / `init` 불변성 구조인가?

### Code-Switching 도메인

- [ ] 파싱이 메시지 저장 후 비동기(큐)로 처리되는가? (응답 경로에 삽입 금지)
- [ ] 결과가 `Message.CodeSwitchData` (JSONB)에 저장되는가?
- [ ] `CodeSwitchSegment`가 record 타입으로 정의되었는가?

### Avatar 도메인

- [ ] `HungerLevel` 변경이 `Feed()` 등 도메인 메서드를 통해서만 이루어지는가?
- [ ] HungerLevel 외부 직접 세터 (`public set`) 가 없는가?
- [ ] `AvatarFedEvent` 등 도메인 이벤트가 발행되는가?
- [ ] HungerLevel 증가 로직이 백그라운드 잡에서만 처리되는가?

### Billing 도메인

- [ ] 토큰 소비가 LLM 응답 수신 후 실제 사용량 기준으로 기록되는가?
- [ ] 요약 배치 잡 토큰이 사용자 `UserQuota`를 소모하지 않는가?
- [ ] 한도 초과 시 `Result.Failure` 반환인가? (Exception throw 금지)

### 도메인 격리 원칙

- [ ] 도메인 간 직접 서비스 호출이 없는가? (도메인 이벤트로만 통신)
- [ ] 도메인 Entity가 다른 도메인의 Entity를 직접 참조하지 않는가?

### Empathy 도메인

- [ ] 공감 모드에서 CodeSwitch 파싱 결과가 응답에 포함되지 않는가?
- [ ] 공감 모드에서 문법 교정/언어 레벨 평가 로직이 차단되는가?
- [ ] 게이지 값(0–100) 범위 검증이 도메인 메서드에서 이루어지는가?
- [ ] 모드가 세션 단위로 유지되고 세션 종료 시 Language로 복귀하는가?
- [ ] 공감 게이지 수치가 LLM 시스템 프롬프트에 명시적으로 포함되는가?

### LLM 연동

- [ ] LLM 호출이 `ILlmService` 인터페이스를 통하는가? (직접 Claude API 호출 금지)
- [ ] `FriendPersona` 성격/말투가 시스템 프롬프트에 포함되는가?

### 타임존 / 날짜

- [ ] 알람/스케줄 관련 날짜 계산에 암산이 없는가? (Bash/Python 도구 사용)
- [ ] `WakeUpAlarm`에 `TimeZoneId` 필드가 있는가?

### 공통 코딩 규칙 (coding-style-dotnet.md 연동)

- [ ] Nullable Reference Types 경고 없음
- [ ] `Result<T>` 패턴 사용 (비즈니스 오류 Exception throw 금지)
- [ ] Entity `private set` 또는 `init` 불변성 확인

## 출력 형식

```
## RianFriends 도메인 검증

**대상 파일**: <파일명>
**도메인**: Memory | CodeSwitch | Avatar | Billing | 격리 | LLM | 공통

### 위반 항목 (있을 경우)

1. [도메인] <위반 내용>
   - 파일: <경로> 라인 <N>
   - 수정 방법: <간단한 수정 가이드>

### 통과 항목
<N>개 항목 통과

### 결론
PASS | FAIL — <한 줄 요약>
```

위반 없으면:
```
## RianFriends 도메인 검증
모든 규칙 통과. PASS
```
