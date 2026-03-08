using RianFriends.Domain.Billing;
using RianFriends.Domain.Identity;

namespace RianFriends.Application.Abstractions;

/// <summary>RevenueCat Webhook 페이로드</summary>
public sealed record RevenueCatWebhookPayload
{
    /// <summary>이벤트 유형 문자열 (예: "INITIAL_PURCHASE")</summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>RevenueCat 사용자 앱 ID (= Supabase User ID)</summary>
    public string AppUserId { get; init; } = string.Empty;

    /// <summary>RevenueCat 고객 ID</summary>
    public string RevenueCatId { get; init; } = string.Empty;

    /// <summary>제품 ID (앱스토어 SKU)</summary>
    public string ProductId { get; init; } = string.Empty;

    /// <summary>구독 만료 예정 일시 (Unix timestamp ms)</summary>
    public long? ExpirationAtMs { get; init; }
}

/// <summary>RevenueCat 관련 비즈니스 서비스 인터페이스</summary>
public interface IBillingService
{
    /// <summary>원시 JSON 페이로드를 파싱합니다.</summary>
    RevenueCatWebhookPayload? ParseWebhook(string rawJson);

    /// <summary>제품 ID를 플랜 유형으로 매핑합니다.</summary>
    PlanType MapProductToPlan(string productId);

    /// <summary>이벤트 유형 문자열을 <see cref="RevenueCatEventType"/>으로 변환합니다.</summary>
    RevenueCatEventType? ParseEventType(string eventType);
}
