using MediatR;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Conversation.Commands.SendMessage;

/// <summary>
/// 메시지를 전송하고 AI 응답을 SSE 스트리밍으로 반환합니다.
/// Controller에서 IAsyncEnumerable로 SSE 응답을 처리합니다.
/// </summary>
public record SendMessageCommand(
    Guid SessionId,
    Guid UserId,
    string Content) : IRequest<Result<IAsyncEnumerable<string>>>;
