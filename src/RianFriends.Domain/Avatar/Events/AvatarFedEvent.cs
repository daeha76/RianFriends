using RianFriends.Domain.Common;

namespace RianFriends.Domain.Avatar.Events;

/// <summary>
/// 아바타에게 먹이를 줬을 때 발행되는 도메인 이벤트.
/// </summary>
public sealed record AvatarFedEvent(Guid AvatarId, int NewHungerLevel) : IDomainEvent;
