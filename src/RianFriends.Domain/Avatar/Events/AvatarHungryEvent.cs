using RianFriends.Domain.Common;

namespace RianFriends.Domain.Avatar.Events;

/// <summary>
/// 아바타의 배고픔 레벨이 70 이상이 됐을 때 발행되는 도메인 이벤트.
/// 푸시 알림 발송 트리거로 사용됩니다.
/// </summary>
public sealed record AvatarHungryEvent(Guid AvatarId, Guid FriendId, int HungerLevel) : IDomainEvent;
