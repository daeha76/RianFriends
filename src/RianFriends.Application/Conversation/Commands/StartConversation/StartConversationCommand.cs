using RianFriends.Application.Abstractions;

namespace RianFriends.Application.Conversation.Commands.StartConversation;

/// <summary>새 대화 세션을 시작합니다. SessionId를 반환합니다.</summary>
public record StartConversationCommand(Guid UserId, Guid FriendId) : ICommand<Guid>;
