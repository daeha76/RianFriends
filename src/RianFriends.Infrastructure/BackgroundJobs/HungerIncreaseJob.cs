using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Abstractions;
using RianFriends.Infrastructure.Persistence;

namespace RianFriends.Infrastructure.BackgroundJobs;

/// <summary>
/// 1시간 주기로 모든 활성 아바타의 배고픔 레벨을 +5 증가시키는 배치 잡.
/// 배고픔 레벨이 70 이상이 되면 INotificationService를 통해 푸시 알림을 발송합니다.
/// </summary>
public sealed class HungerIncreaseJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HungerIncreaseJob> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);
    private const int HungerIncreaseAmount = 5;

    /// <inheritdoc />
    public HungerIncreaseJob(IServiceScopeFactory scopeFactory, ILogger<HungerIncreaseJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("HungerIncreaseJob 시작 — 1시간마다 배고픔 +{Amount}", HungerIncreaseAmount);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(CheckInterval, stoppingToken);

            try
            {
                await ProcessHungerIncreaseAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "HungerIncreaseJob 실행 중 오류 발생");
            }
        }
    }

    private async Task ProcessHungerIncreaseAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        // 모든 아바타 목록 조회
        var avatars = await context.Avatars.ToListAsync(ct);
        _logger.LogInformation("배고픔 처리 대상 아바타: {Count}개", avatars.Count);

        // 배고픔이 70을 넘는 아바타의 userId를 알기 위한 친구 목록 조회
        var friendIds = avatars
            .Select(a => a.FriendId)
            .ToList();

        var friends = await context.Friends
            .Where(f => friendIds.Contains(f.Id) && f.IsActive)
            .ToDictionaryAsync(f => f.Id, f => f.UserId, ct);

        foreach (var avatar in avatars)
        {
            var wasBelow70 = avatar.HungerLevel < 70;
            avatar.IncreaseHunger(HungerIncreaseAmount);

            // 70 임계치를 처음 넘었을 때만 알림 발송
            if (wasBelow70 && avatar.HungerLevel >= 70)
            {
                if (friends.TryGetValue(avatar.FriendId, out var userId))
                {
                    try
                    {
                        await notificationService.SendHungerAlertAsync(userId, avatar.FriendId, avatar.HungerLevel, ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "배고픔 알림 발송 실패: AvatarId={AvatarId}",
                            avatar.Id);
                    }
                }
            }
        }

        await context.SaveChangesAsync(ct);
        _logger.LogInformation("HungerIncreaseJob 사이클 완료");
    }
}
