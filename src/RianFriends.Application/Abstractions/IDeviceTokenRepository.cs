using RianFriends.Domain.Notification;

namespace RianFriends.Application.Abstractions;

/// <summary>디바이스 토큰 영속성 추상화</summary>
public interface IDeviceTokenRepository
{
    /// <summary>사용자의 활성 디바이스 토큰 목록을 조회합니다.</summary>
    Task<List<DeviceToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken ct);

    /// <summary>디바이스 토큰을 추가합니다.</summary>
    void Add(DeviceToken deviceToken);

    /// <summary>변경사항을 저장합니다.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct);
}
