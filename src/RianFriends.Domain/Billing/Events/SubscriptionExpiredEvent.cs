using RianFriends.Domain.Common;

namespace RianFriends.Domain.Billing.Events;

/// <summary>구독 만료/취소 도메인 이벤트</summary>
public sealed record SubscriptionExpiredEvent(Guid UserId, Guid SubscriptionId) : IDomainEvent;
