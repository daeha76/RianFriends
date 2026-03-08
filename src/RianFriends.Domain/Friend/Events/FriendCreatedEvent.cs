using RianFriends.Domain.Common;

namespace RianFriends.Domain.Friend.Events;

/// <summary>새 AI 친구가 생성될 때 발행되는 도메인 이벤트</summary>
public sealed record FriendCreatedEvent(Guid FriendId, Guid UserId, Guid PersonaId) : IDomainEvent;
