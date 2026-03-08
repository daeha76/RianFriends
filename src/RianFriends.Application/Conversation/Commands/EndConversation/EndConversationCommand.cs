using MediatR;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Conversation.Commands.EndConversation;

/// <summary>대화 세션을 종료합니다. 공감 모드는 Language로 복귀합니다.</summary>
public record EndConversationCommand(Guid SessionId, Guid UserId) : IRequest<Result>;
