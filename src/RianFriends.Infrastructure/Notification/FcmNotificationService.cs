using Microsoft.Extensions.Logging;
using RianFriends.Application.Abstractions;

namespace RianFriends.Infrastructure.Notification;

/// <summary>
/// INotificationService의 Stub 구현체.
/// Phase 3에서는 로그 출력만 수행합니다.
/// Phase 4에서 FCM HTTP v1 API로 교체 예정.
/// </summary>
internal sealed class FcmNotificationService : INotificationService
{
    private readonly ILogger<FcmNotificationService> _logger;

    /// <inheritdoc />
    public FcmNotificationService(ILogger<FcmNotificationService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task SendHungerAlertAsync(Guid userId, Guid friendId, int hungerLevel, CancellationToken ct)
    {
        // TODO Phase 4: FCM HTTP v1 API 연동
        _logger.LogWarning(
            "[STUB] 배고픔 알림 발송 요청 — UserId={UserId}, FriendId={FriendId}, HungerLevel={HungerLevel}",
            userId,
            friendId,
            hungerLevel);

        return Task.CompletedTask;
    }
}
