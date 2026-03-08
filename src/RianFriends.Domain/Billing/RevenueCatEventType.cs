namespace RianFriends.Domain.Billing;

/// <summary>RevenueCat Webhook 이벤트 유형</summary>
public enum RevenueCatEventType
{
    /// <summary>초기 구매</summary>
    InitialPurchase,

    /// <summary>구독 갱신</summary>
    Renewal,

    /// <summary>제품 변경 (업그레이드/다운그레이드)</summary>
    ProductChange,

    /// <summary>구독 취소 (기간 만료 전 사용자 취소)</summary>
    Cancellation,

    /// <summary>구독 만료</summary>
    Expiration,

    /// <summary>결제 오류</summary>
    BillingIssue,

    /// <summary>구독자 별칭 설정</summary>
    SubscriberAlias
}
