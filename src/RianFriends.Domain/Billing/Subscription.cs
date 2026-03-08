using RianFriends.Domain.Billing.Events;
using RianFriends.Domain.Common;
using RianFriends.Domain.Identity;

namespace RianFriends.Domain.Billing;

/// <summary>
/// RevenueCat 구독 이력 엔티티.
/// 구독 활성화/만료/취소 상태를 추적합니다.
/// </summary>
public sealed class Subscription : AuditableEntity
{
    /// <summary>사용자 ID</summary>
    public Guid UserId { get; private set; }

    /// <summary>RevenueCat 고객 ID</summary>
    public string RevenueCatCustomerId { get; private set; } = string.Empty;

    /// <summary>앱스토어 제품 ID (예: com.rianfriends.basic_monthly)</summary>
    public string ProductId { get; private set; } = string.Empty;

    /// <summary>매핑된 플랜 유형</summary>
    public PlanType PlanType { get; private set; }

    /// <summary>구독 시작 일시</summary>
    public DateTimeOffset StartsAt { get; private set; }

    /// <summary>구독 만료 예정 일시</summary>
    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>현재 활성 여부</summary>
    public bool IsActive { get; private set; }

    /// <summary>EF Core용 기본 생성자</summary>
    private Subscription() { }

    /// <summary>신규 구독을 생성합니다.</summary>
    /// <param name="userId">사용자 ID</param>
    /// <param name="revenueCatCustomerId">RevenueCat 고객 ID</param>
    /// <param name="productId">제품 ID</param>
    /// <param name="planType">플랜 유형</param>
    /// <param name="expiresAt">만료 예정 일시</param>
    public static Result<Subscription> Create(
        Guid userId,
        string revenueCatCustomerId,
        string productId,
        PlanType planType,
        DateTimeOffset expiresAt)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<Subscription>("사용자 ID가 유효하지 않습니다.");
        }

        if (string.IsNullOrWhiteSpace(revenueCatCustomerId))
        {
            return Result.Failure<Subscription>("RevenueCat 고객 ID는 필수입니다.");
        }

        if (string.IsNullOrWhiteSpace(productId))
        {
            return Result.Failure<Subscription>("제품 ID는 필수입니다.");
        }

        if (expiresAt <= DateTimeOffset.UtcNow)
        {
            return Result.Failure<Subscription>("만료 일시는 현재 시각 이후여야 합니다.");
        }

        var subscription = new Subscription
        {
            UserId = userId,
            RevenueCatCustomerId = revenueCatCustomerId,
            ProductId = productId,
            PlanType = planType,
            StartsAt = DateTimeOffset.UtcNow,
            ExpiresAt = expiresAt,
            IsActive = true
        };

        subscription.AddDomainEvent(new SubscriptionActivatedEvent(userId, planType));

        return Result.Success(subscription);
    }

    /// <summary>
    /// 구독을 비활성화합니다 (만료 또는 취소).
    /// <see cref="SubscriptionExpiredEvent"/>를 발행합니다.
    /// </summary>
    public Result Deactivate()
    {
        if (!IsActive)
        {
            return Result.Failure("이미 비활성화된 구독입니다.");
        }

        IsActive = false;
        AddDomainEvent(new SubscriptionExpiredEvent(UserId, Id));

        return Result.Success();
    }

    /// <summary>구독을 갱신합니다 (동일 제품 갱신).</summary>
    /// <param name="newExpiresAt">갱신된 만료 일시</param>
    public void Renew(DateTimeOffset newExpiresAt)
    {
        ExpiresAt = newExpiresAt;
        IsActive = true;
    }
}
