using MediatR;
using RianFriends.Application.Abstractions;
using RianFriends.Application.Avatar.Dtos;
using RianFriends.Domain.Avatar;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Avatar.Queries.GetAvatarState;

/// <summary>아바타 상태 조회 핸들러. 아바타가 없으면 기본값(배고픔 0)으로 응답합니다.</summary>
public sealed class GetAvatarStateQueryHandler : IRequestHandler<GetAvatarStateQuery, Result<AvatarStateDto>>
{
    private readonly IAvatarRepository _avatarRepository;

    /// <inheritdoc />
    public GetAvatarStateQueryHandler(IAvatarRepository avatarRepository)
    {
        _avatarRepository = avatarRepository;
    }

    /// <inheritdoc />
    public async Task<Result<AvatarStateDto>> Handle(GetAvatarStateQuery request, CancellationToken cancellationToken)
    {
        if (request.FriendId == Guid.Empty)
        {
            return Result.Failure<AvatarStateDto>("친구 ID는 필수입니다.");
        }

        var avatar = await _avatarRepository.GetByFriendIdAsync(request.FriendId, cancellationToken);

        if (avatar is null)
        {
            // 아직 먹이를 한 번도 안 준 친구는 기본 상태(배부름) 반환
            return Result.Success(new AvatarStateDto(
                request.FriendId,
                HungerLevel: 0,
                HungerStatus: "Satisfied",
                LastFedAt: DateTimeOffset.UtcNow));
        }

        return Result.Success(new AvatarStateDto(
            avatar.FriendId,
            avatar.HungerLevel,
            avatar.HungerStatus.ToString(),
            avatar.LastFedAt));
    }
}
