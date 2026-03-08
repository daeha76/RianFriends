using RianFriends.Domain.Common;

namespace RianFriends.Domain.Identity.Events;

/// <summary>사용자 탈퇴 도메인 이벤트</summary>
public sealed record UserDeletedEvent(Guid UserId) : IDomainEvent;
