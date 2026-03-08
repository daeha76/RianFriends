using MediatR;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Abstractions;
using RianFriends.Application.Identity.Interfaces;
using RianFriends.Domain.Common;
using RianFriends.Domain.Friend;
using RianFriends.Domain.Identity;

namespace RianFriends.Application.Friend.Commands.CreateFriend;

/// <summary>
/// AI 친구 생성 핸들러.
/// 플랜별 최대 친구 수를 초과하면 실패를 반환합니다.
/// </summary>
public sealed class CreateFriendCommandHandler : IRequestHandler<CreateFriendCommand, Result<Guid>>
{
    private readonly IFriendRepository _friendRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<CreateFriendCommandHandler> _logger;

    // 플랜별 최대 친구 수
    private static readonly Dictionary<PlanType, int> MaxFriendsByPlan = new()
    {
        { PlanType.Free, 1 },
        { PlanType.Basic, 3 },
        { PlanType.Plus, 5 },
        { PlanType.Pro, int.MaxValue }
    };

    /// <inheritdoc />
    public CreateFriendCommandHandler(
        IFriendRepository friendRepository,
        IUserRepository userRepository,
        ILogger<CreateFriendCommandHandler> logger)
    {
        _friendRepository = friendRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreateFriendCommand request, CancellationToken cancellationToken)
    {
        // 1. 페르소나 존재 여부 확인
        var persona = await _friendRepository.GetPersonaByIdAsync(request.PersonaId, cancellationToken);
        if (persona is null)
        {
            _logger.LogWarning("페르소나를 찾을 수 없음: {PersonaId}", request.PersonaId);
            return Result.Failure<Guid>("존재하지 않는 페르소나입니다.");
        }

        // 2. 사용자 플랜 조회
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<Guid>("사용자를 찾을 수 없습니다.");
        }

        // 3. 현재 친구 수 확인
        var currentCount = await _friendRepository.CountActiveByUserIdAsync(request.UserId, cancellationToken);
        var maxCount = MaxFriendsByPlan.TryGetValue(user.Plan, out var max) ? max : 1;

        // 4. 친구 생성 (플랜 제한 내부에서 검사)
        var friendResult = Domain.Friend.Friend.Create(request.UserId, request.PersonaId, currentCount, maxCount);
        if (friendResult.IsFailure)
        {
            return Result.Failure<Guid>(friendResult.Error);
        }

        // 5. 저장
        _friendRepository.Add(friendResult.Value);
        await _friendRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("AI 친구 생성: UserId={UserId}, FriendId={FriendId}", request.UserId, friendResult.Value.Id);
        return Result.Success(friendResult.Value.Id);
    }
}
