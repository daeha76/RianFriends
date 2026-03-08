using MediatR;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Avatar;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Avatar.Commands.FeedAvatar;

/// <summary>
/// 아바타 먹이기 핸들러.
/// 아바타 조회 → Feed() 호출 → Snack 기록 → DB 저장.
/// </summary>
public sealed class FeedAvatarCommandHandler : IRequestHandler<FeedAvatarCommand, Result<int>>
{
    private readonly IAvatarRepository _avatarRepository;
    private readonly ILogger<FeedAvatarCommandHandler> _logger;

    /// <inheritdoc />
    public FeedAvatarCommandHandler(
        IAvatarRepository avatarRepository,
        ILogger<FeedAvatarCommandHandler> logger)
    {
        _avatarRepository = avatarRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<int>> Handle(FeedAvatarCommand request, CancellationToken cancellationToken)
    {
        var avatar = await _avatarRepository.GetByFriendIdAsync(request.FriendId, cancellationToken);

        if (avatar is null)
        {
            // 아바타가 없으면 자동 생성 (친구 생성 시 아직 아바타가 없을 수 있음)
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

        // 간식 이력 기록
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
