namespace RianFriends.Domain.Memory;

/// <summary>
/// 계층적 메모리 레이어. 시간 범위에 따라 요약 수준이 달라집니다.
/// 실시간 대화 응답에는 ShortTerm + MidTerm만 컨텍스트로 주입합니다.
/// </summary>
public enum MemoryLayer
{
    /// <summary>단기 기억 (7일) — 최신 대화 원문 보존</summary>
    ShortTerm,

    /// <summary>중기 기억 (30일) — 주제별 요약</summary>
    MidTerm,

    /// <summary>분기 기억 (3개월) — 관계 패턴, 관심사</summary>
    Quarter,

    /// <summary>반기 기억 (6개월) — 주요 사건, 감정 기록</summary>
    HalfYear,

    /// <summary>연간 기억 (1년) — 연간 회고 요약</summary>
    Annual,

    /// <summary>10년 기억 (10년) — 핵심 관계 정체성 요약</summary>
    Decade
}
