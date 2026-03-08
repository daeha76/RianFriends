# Domain: RianFriends — AI 친구 앱 (개요)

> 상세 규칙은 분리된 파일 참조:
> - `domain-memory-learning.md` — Memory 계층 전략, Code-Switching, 언어 레벨
> - `domain-avatar-empathy.md` — Avatar, Empathy 공감 모드, Notification
> - `domain-billing-llm.md` — Billing/플랜, LLM API 선택 기준, 소셜 인증

---

## 비즈니스 컨텍스트

외국인 AI 친구와 대화하고, 아바타를 꾸미고, 간식을 주거나 아침 알람을 받을 수 있는 감성 AI 친구 앱.
대상: 언어 학습에 관심 있는 사용자 (한국어 ↔ 외국어 혼용 대화 지원).

---

## 핵심 도메인 구조

```
User (사용자)
  └── Friend (AI 친구)
        ├── Avatar              # 아바타 외형/상태 (배고픔, 기분 등)
        ├── Memory[]            # 계층적 대화 메모리
        ├── Conversation[]      # 대화 이력
        └── Notification[]      # 알람 설정 (기상 알람 등)
```

---

## 핵심 용어 (코드 네이밍에 반영)

| 한국어 | 영문 (코드) | 설명 |
|:-------|:-----------|:-----|
| AI 친구 | `Friend` | 사용자마다 1명 이상의 AI 친구 |
| 아바타 | `Avatar` | 친구의 외형 및 감정 상태 표현 객체 |
| 대화 | `Conversation` | 사용자와 AI 친구 간의 대화 세션 |
| 메시지 | `Message` | 대화 내 개별 발화 단위 |
| 기억 | `Memory` | 대화 내용을 요약·압축한 장기 기억 |
| 코드스위칭 | `CodeSwitch` | 혼용 언어 파싱 결과 (원문, 병음, 뜻) |
| 간식 | `Snack` | 아바타에게 주는 아이템 (배고픔 해소) |
| 배고픔 | `HungerLevel` | 아바타 상태 수치 (0–100) |
| 기상 알람 | `WakeUpAlarm` | 매일 아침 친구가 깨워주는 알림 |
| 기억 레이어 | `MemoryLayer` | 시간 범위별 메모리 계층 |

---

## 모듈 격리 원칙

모든 기능은 도메인 단위로 격리. 도메인 간 직접 참조 금지 — 도메인 이벤트로 통신:

| 도메인 | 책임 |
|:-------|:-----|
| `Identity` | 사용자 계정, 인증 |
| `Friend` | AI 친구 생성/설정, 성격 관리 |
| `Memory` | 대화 메모리 계층 관리, 요약 생성 |
| `Conversation` | 대화 세션, 메시지 저장, LLM 호출 |
| `Learning` | Code-Switching 파싱, 단어장 |
| `Avatar` | 아바타 상태, 간식, 외형 커스터마이징 |
| `Notification` | 기상 알람, 푸시 알림 발송 |

---

## Claude 행동 규칙

1. 메모리 요약 로직은 **절대 실시간 대화 흐름에 삽입 금지** — 배치 잡으로만
2. 아바타 상태(HungerLevel) 변경은 **반드시 도메인 메서드**를 통해서만
3. 언어 파싱(Code-Switching)은 **비동기 후처리** — 메시지 응답 후 별도 큐에서 처리
4. 도메인 간 참조는 **도메인 이벤트**로만 — 직접 서비스 호출 금지
5. 타임존이 관련된 알람/스케줄은 **date-calculation.md 규칙** 적용 (암산 금지)
6. 토큰 소비 기록은 **LLM 응답 완료 후** — 요청 전 선차감 금지
7. `FriendPersona` 성격/말투는 **모든 대화 응답 시스템 프롬프트에 반드시 포함**
8. 언어 레벨 재평가는 **백그라운드 잡**에서만 — 실시간 대화 중 평가 금지
9. 공감 모드(`ConversationMode.Empathy`)에서는 **CodeSwitch 결과, 문법 교정, 언어 레벨 평가 모두 차단**
10. 공감 게이지는 **응답 생성 시스템 프롬프트에 수치로 명시** — "현재 공감 게이지: 80, 응답의 80%는 감정 공감으로 구성"
11. LLM 모델 변경 시 `ILlmService` 구현체만 교체 — Application/Domain 레이어 코드 변경 금지
