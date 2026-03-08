using MediatR;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Avatar;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Avatar.Commands.FeedAvatar;

/// <summary>
/// 아바타 먹이기 핸들러.
/// 소유권 검증 → 아바타 조회 → Feed() 호출 → Snack 기록 → DB 저장.
/// </summary>
public sealed class FeedAvatarCommandHandler : IRequestHandler<FeedAvatarCommand, Result<int>>
{
    private readonly IAvatarRepository _avatarRepository;
    private readonly IFriendRepository _friendRepository;
    private readonly ILogger<FeedAvatarCommandHandler> _logger;

    /// <inheritdoc />
    public FeedAvatarCommandHandler(
        IAvatarRepository avatarRepository,
        IFriendRepository friendRepository,
        ILogger<FeedAvatarCommandHandler> logger)
    {
        _avatarRepository = avatarRepository;
        _friendRepository = friendRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<int>> Handle(FeedAvatarCommand request, CancellationToken cancellationToken)
    {
        // 소유권 검증
        var friend = await _friendRepository.GetByIdAsync(request.FriendId, cancellationToken);
        if (friend is null || friend.UserId != request.UserId)
        {
            return Result.Failure<int>("해당 친구를 찾을 수 없습니다.");
        }

        var avatar = await _avatarRepository.GetByFriendIdAsync(request.FriendId, cancellationToken);

        if (avatar is null)
        {
            var createResult = Domain.Avatar.Avatar.Create(request.FriendId);
            if (createResult.IsFailure)
            {
                return Result.Failure<int>(createResult.Error);
            }

            avatar = createResult.Value;
            _avatarRepository.Add(avatar);
        }

        var feedResult = avatar.Feed();
        if (feedResult.IsFailure)
        {
            return Result.Failure<int>(feedResult.Error);
        }

        var snackResult = Snack.Create(avatar.Id, request.SnackType);
        if (snackResult.IsSuccess)
        {
            _avatarRepository.AddSnack(snackResult.Value);
        }

        await _avatarRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "아바타 먹이기 완료: FriendId={FriendId}, NewHungerLevel={HungerLevel}",
            request.FriendId,
            avatar.HungerLevel);

        return Result.Success(avatar.HungerLevel);
    }
}
