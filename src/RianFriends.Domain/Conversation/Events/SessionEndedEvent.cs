using RianFriends.Domain.Common;

namespace RianFriends.Domain.Conversation.Events;

/// <summary>대화 세션이 종료될 때 발행되는 도메인 이벤트. 언어 레벨 재평가 트리거로 사용.</summary>
public sealed record SessionEndedEvent(
    Guid SessionId,
    Guid UserId,
    Guid FriendId,
    int SessionNumber) : IDomainEvent;
