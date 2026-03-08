using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Billing.Commands.ResetDailyQuota;

namespace RianFriends.Infrastructure.BackgroundJobs;

/// <summary>
/// 매일 UTC 00:00에 모든 사용자의 일일 토큰 쿼터를 초기화하는 배치 잡.
/// </summary>
public sealed class DailyQuotaResetJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DailyQuotaResetJob> _logger;

    /// <inheritdoc />
    public DailyQuotaResetJob(IServiceScopeFactory scopeFactory, ILogger<DailyQuotaResetJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DailyQuotaResetJob 시작 — 매일 UTC 00:00에 쿼터 초기화");

        while (!stoppingToken.IsCancellationRequested)
        {
            // 다음 UTC 자정까지 대기
            var now = DateTime.UtcNow;
            var nextMidnight = now.Date.AddDays(1);
            var delay = nextMidnight - now;

            _logger.LogInformation("다음 쿼터 초기화: {NextMidnight:yyyy-MM-dd HH:mm:ss} UTC (대기: {Delay})", nextMidnight, delay);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                await ResetQuotasAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "DailyQuotaResetJob 실행 중 오류 발생");
            }
        }
    }

    private async Task ResetQuotasAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await mediator.Send(new ResetDailyQuotaCommand(), ct);

        if (result.IsFailure)
        {
            _logger.LogError("DailyQuotaResetJob 실패: {Error}", result.Error);
        }
        else
        {
            _logger.LogInformation("DailyQuotaResetJob 완료 — 쿼터 초기화 성공");
        }
    }
}
