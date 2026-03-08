using RianFriends.Domain.Common;

namespace RianFriends.Domain.Identity.Events;

/// <summary>사용자 플랜 변경 도메인 이벤트</summary>
public sealed record UserPlanChangedEvent(Guid UserId, PlanType NewPlan) : IDomainEvent;
