using MediatR;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Abstractions;
using RianFriends.Application.Identity.Interfaces;
using RianFriends.Domain.Billing;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Billing.Queries.CheckTokenQuota;

/// <summary>
/// 토큰 쿼터 확인 핸들러.
/// 오늘 사용 가능한 토큰이 충분한지 확인합니다. 쿼터가 없으면 자동 생성합니다.
/// </summary>
public sealed class CheckTokenQuotaQueryHandler
    : IRequestHandler<CheckTokenQuotaQuery, Result<bool>>
{
    private readonly IUserQuotaRepository _quotaRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<CheckTokenQuotaQueryHandler> _logger;

    /// <inheritdoc />
    public CheckTokenQuotaQueryHandler(
        IUserQuotaRepository quotaRepository,
        IUserRepository userRepository,
        ILogger<CheckTokenQuotaQueryHandler> logger)
    {
        _quotaRepository = quotaRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<bool>> Handle(
        CheckTokenQuotaQuery request,
        CancellationToken cancellationToken)
    {
        var quota = await _quotaRepository.GetTodayAsync(request.UserId, cancellationToken);

        if (quota is null)
        {
            // 오늘 쿼터가 없으면 사용자의 현재 플랜으로 생성
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                return Result.Failure<bool>("사용자를 찾을 수 없습니다.");
            }

            var createResult = UserQuota.Create(request.UserId, user.Plan);
            if (createResult.IsFailure)
            {
                return Result.Failure<bool>(createResult.Error);
            }

            quota = createResult.Value;
            _quotaRepository.Add(quota);
            await _quotaRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("일일 쿼터 생성: UserId={UserId}, Limit={Limit}", request.UserId, quota.QuotaLimit);
        }

        // 잔여 토큰 확인 (실제 소비는 하지 않음)
        var remaining = quota.QuotaLimit - quota.UsedTokens;
        var hasEnough = remaining >= request.RequiredTokens;

        if (!hasEnough)
        {
            _logger.LogInformation(
                "토큰 한도 초과: UserId={UserId}, Required={Required}, Remaining={Remaining}",
                request.UserId, request.RequiredTokens, remaining);
        }

        return Result.Success(hasEnough);
    }
}
