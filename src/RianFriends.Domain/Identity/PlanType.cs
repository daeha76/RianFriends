namespace RianFriends.Domain.Identity;

/// <summary>구독 플랜 유형</summary>
public enum PlanType
{
    /// <summary>무료 플랜 (친구 1명, 일 3,000 토큰)</summary>
    Free,

    /// <summary>베이직 플랜 (친구 3명, 일 20,000 토큰)</summary>
    Basic,

    /// <summary>플러스 플랜 (친구 5명, 일 100,000 토큰)</summary>
    Plus,

    /// <summary>프로 플랜 (친구 무제한, 토큰 무제한)</summary>
    Pro
}
