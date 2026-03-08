namespace RianFriends.Application.Abstractions;

/// <summary>푸시 알림 발송 서비스 추상화 (FCM / APNs)</summary>
public interface INotificationService
{
    /// <summary>
    /// 배고픔 알림을 해당 사용자의 모든 활성 디바이스로 발송합니다.
    /// </summary>
    /// <param name="userId">알림 수신 사용자 ID</param>
    /// <param name="friendId">배고픈 AI 친구 ID</param>
    /// <param name="hungerLevel">현재 배고픔 레벨 (0–100)</param>
    /// <param name="ct">취소 토큰</param>
    Task SendHungerAlertAsync(Guid userId, Guid friendId, int hungerLevel, CancellationToken ct);
}
