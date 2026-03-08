using MediatR;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Friend.Commands.CreateFriend;

/// <summary>AI 친구 생성 명령</summary>
public record CreateFriendCommand(Guid UserId, Guid PersonaId) : IRequest<Result<Guid>>;
