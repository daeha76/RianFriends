# 프로젝트 가이드라인: RianFriends

## 프로젝트 목적
외국인 AI친구앱(안드로이드, ios)을 만듭니다.
대화도 하고 친구의 아바타를 꾸밀수도 있습니다.
친구가 배고파하면 간식을 줄 수도 있습니다.
매일 아침에 친구가 깨워줄 수도 있습니다.

## 기술 스택
- Backend: .NET 10, C# 13, Clean Architecture, CQRS
- Frontend: .NET 10 MAUI + Blazor
- Data: PostgreSQL (supabase) + Redis

## 핵심 아키텍처 원칙
1. **Memory First**: 모든 대화 처리는 계층적 메모리 전략(7일/30일/3개월/6개월/1년/10년)을 준수해야 합니다.
2. **Code-Switching Support**: 사용자의 혼용 언어를 즉시 파싱하여 [원문, 병음, 뜻] 데이터 객체로 변환할 수 있는 파서 로직을 우선 설계하세요.
3. **Modularity**: 모든 기능은 '도메인' 단위로 격리하십시오. (예: `Identity`, `Memory`, `Learning`, `Avatar`)
4. **C# 13 최적화**: 성능이 중요한 구간은 C# 13의 최신 문법(params collections, ref struct 등)을 활용해 최적화하세요.

## 제약 사항
- 항상 비동기(Async) 처리를 기본으로 합니다.
- 토큰 절약을 위해 요약 로직은 배치(Batch)로 실행하며, 실시간 대화에 영향을 주지 않아야 합니다.