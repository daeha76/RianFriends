using MediatR;
using RianFriends.Application.Abstractions;
using RianFriends.Application.Avatar.Dtos;
using RianFriends.Domain.Avatar;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Avatar.Queries.GetAvatarState;

/// <summary>아바타 상태 조회 핸들러. 소유권 검증 후 아바타 상태를 반환합니다.</summary>
public sealed class GetAvatarStateQueryHandler : IRequestHandler<GetAvatarStateQuery, Result<AvatarStateDto>>
{
    private readonly IAvatarRepository _avatarRepository;
    private readonly IFriendRepository _friendRepository;

    /// <inheritdoc />
    public GetAvatarStateQueryHandler(IAvatarRepository avatarRepository, IFriendRepository friendRepository)
    {
        _avatarRepository = avatarRepository;
        _friendRepository = friendRepository;
    }

    /// <inheritdoc />
    public async Task<Result<AvatarStateDto>> Handle(GetAvatarStateQuery request, CancellationToken cancellationToken)
    {
        // 소유권 검증: 해당 Friend가 요청 사용자의 것인지 확인
        var friend = await _friendRepository.GetByIdAsync(request.FriendId, cancellationToken);
        if (friend is null || friend.UserId != request.UserId)
        {
            return Result.Failure<AvatarStateDto>("해당 친구를 찾을 수 없습니다.");
        }

        var avatar = await _avatarRepository.GetByFriendIdAsync(request.FriendId, cancellationToken);

        if (avatar is null)
        {
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
