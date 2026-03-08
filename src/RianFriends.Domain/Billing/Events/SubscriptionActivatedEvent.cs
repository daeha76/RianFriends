using RianFriends.Domain.Common;
using RianFriends.Domain.Identity;

namespace RianFriends.Domain.Billing.Events;

/// <summary>구독 활성화 도메인 이벤트</summary>
public sealed record SubscriptionActivatedEvent(Guid UserId, PlanType NewPlan) : IDomainEvent;
