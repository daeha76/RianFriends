using MediatR;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Billing.Commands.ResetDailyQuota;

/// <summary>
/// 일일 쿼터 초기화 핸들러.
/// 배치 잡(DailyQuotaResetJob)에서 호출되어 전체 쿼터를 리셋합니다.
/// </summary>
public sealed class ResetDailyQuotaCommandHandler
    : IRequestHandler<ResetDailyQuotaCommand, Result>
{
    private readonly IUserQuotaRepository _quotaRepository;
    private readonly ILogger<ResetDailyQuotaCommandHandler> _logger;

    /// <inheritdoc />
    public ResetDailyQuotaCommandHandler(
        IUserQuotaRepository quotaRepository,
        ILogger<ResetDailyQuotaCommandHandler> logger)
    {
        _quotaRepository = quotaRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(
        ResetDailyQuotaCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("일일 쿼터 초기화 시작 (UTC {Date})", DateOnly.FromDateTime(DateTime.UtcNow));

        await _quotaRepository.ResetAllAsync(cancellationToken);

        _logger.LogInformation("일일 쿼터 초기화 완료");
        return Result.Success();
    }
}
