---
name: explain-code
description: >
  Explain code with analogies, layer context, and structured output.
  Use when: "코드 설명해", "이 코드 뭐해", "이 부분 이해가 안 돼", "어떻게 동작해",
  "explain this code", "explain this", "what does this do", "코드 분석해",
  "이 함수 설명", "이 클래스 설명", "이 컴포넌트 설명".
---

# Explain Code

## Intent

코드 설명 요청 시 자동 적용. 파일 확장자 또는 컨텍스트로 모드를 판별하고,
비유 먼저 + 결론 먼저 원칙에 따라 구조화된 설명을 제공한다.

## 모드 판별

| 모드 | 감지 조건 |
|------|----------|
| **C# / .NET** | `.cs` 확장자, CQRS/Command/Query/Handler/Domain/Repository 언급 |
| **Blazor / MAUI** | `.razor`, `.xaml` 확장자, 컴포넌트/페이지/렌더링 언급 |
| **SQL / EF Core** | `.sql` 확장자, Migration/DbContext/LINQ/Entity 언급 |
| **범용** | 위 조건 미해당 시 |

## 설명 원칙

1. **비유 먼저** — 기술 개념을 일상 비유 1-2문장으로 먼저 설명
2. **결론 먼저** — 핵심 역할 한 줄 요약 후 세부 동작 전개
3. **레이어 맥락 명시** (C# 모드) — Domain / Application / Infrastructure / Presentation 중 위치
4. **사이드 이펙트 강조** — 상태 변경, DB 쓰기, Domain Event 발행 여부를 명시
5. **렌더링 모드 명시** (Blazor 모드) — SSR / Server / WASM / Auto 중 적용 모드

## C# / .NET 모드

### 설명 흐름

1. 한 줄 요약 (이 클래스/메서드의 핵심 역할)
2. 비유 (일상적 비유로 추상화)
3. Clean Architecture 레이어 위치
4. 코드 동작 흐름 (단계별)
5. 사이드 이펙트 (있을 경우만)

### CQRS 패턴 주의사항

- **Command Handler**: 상태 변경 + Domain Event 발행 여부 명시
- **Query Handler**: 읽기 전용임을 명시, side-effect 없음 강조
- **Domain Entity**: `private set` / `init` 불변성 구조 설명

## Blazor / MAUI 모드

### 설명 흐름

1. 한 줄 요약
2. 비유
3. 렌더링 모드 (SSR / Server / WASM / Auto)
4. 라이프사이클 메서드 실행 순서
5. JS Interop 사용 여부 (있을 경우 OnAfterRenderAsync 제약 설명)
6. IDisposable 구현 여부

### 라이프사이클 순서 참조

```
SetParametersAsync → OnInitialized(Async) → OnParametersSet(Async)
→ BuildRenderTree → OnAfterRender(Async) → [상호작용] → DisposeAsync
```

## SQL / EF Core 모드

### 설명 흐름

1. 한 줄 요약 (이 쿼리/마이그레이션의 목적)
2. 비유
3. 실행 순서 (EF Core → SQL 변환 → DB 실행)
4. 인덱스/RLS 정책 관련 주의사항 (있을 경우만)
5. N+1 쿼리 또는 성능 주의사항 (있을 경우만)

## 출력 형식

### C# / .NET

```
## 코드 설명: <파일명 또는 클래스/메서드명>

**한 줄 요약**: <핵심 역할>

**비유**: <일상 비유>

**레이어**: Domain | Application | Infrastructure | Presentation

**동작 흐름**:
1. <단계>
2. <단계>
3. <단계>

**사이드 이펙트** (있을 경우만):
- DB 쓰기: 있음 / 없음
- Domain Event: <이벤트명> 발행 / 없음
- 외부 API 호출: 있음 / 없음
```

### Blazor / MAUI

```
## 코드 설명: <컴포넌트명>

**한 줄 요약**: <핵심 역할>

**비유**: <일상 비유>

**렌더링 모드**: SSR | Server | WASM | Auto

**라이프사이클**:
1. <실행 메서드> — <설명>
2. <실행 메서드> — <설명>

**주의할 점** (있을 경우만):
- <JS Interop 제약, IDisposable 누락, FormName 누락 등>
```

### SQL / EF Core

```
## 코드 설명: <마이그레이션명 또는 쿼리>

**한 줄 요약**: <이 쿼리/마이그레이션이 하는 일>

**비유**: <일상 비유>

**실행 흐름**:
1. <단계>
2. <단계>

**주의할 점** (있을 경우만):
- <인덱스 누락, RLS 정책, N+1 위험 등>
```

## 범용 모드

위 3가지 모드에 해당하지 않을 때는 아래 간소 형식을 사용:

```
**한 줄 요약**: <핵심 역할>

**비유**: <일상 비유>

**동작 흐름**:
1. <단계>
2. <단계>
```
