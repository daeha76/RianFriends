using RianFriends.Domain.Common;

namespace RianFriends.Domain.Conversation.Events;

/// <summary>메시지가 전송되고 저장될 때 발행되는 도메인 이벤트</summary>
public sealed record MessageSentEvent(Guid MessageId, Guid SessionId, string Role) : IDomainEvent;
