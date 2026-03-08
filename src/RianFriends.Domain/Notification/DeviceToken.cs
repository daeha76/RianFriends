using RianFriends.Domain.Common;

namespace RianFriends.Domain.Notification;

/// <summary>디바이스 플랫폼</summary>
public enum DevicePlatform
{
    /// <summary>iOS / iPadOS</summary>
    Ios = 0,

    /// <summary>Android</summary>
    Android = 1,
}

/// <summary>
/// 푸시 알림을 위한 디바이스 토큰 엔티티.
/// FCM(Android) 및 APNs(iOS) 토큰을 저장합니다.
/// </summary>
public sealed class DeviceToken : AuditableEntity
{
    /// <summary>토큰 소유 사용자 ID</summary>
    public Guid UserId { get; private set; }

    /// <summary>FCM 또는 APNs 토큰 값</summary>
    public string Token { get; private set; } = string.Empty;

    /// <summary>디바이스 플랫폼</summary>
    public DevicePlatform Platform { get; private set; }

    /// <summary>활성 여부 (로그아웃 또는 토큰 갱신 시 비활성화)</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>EF Core용 기본 생성자</summary>
    private DeviceToken() { }

    /// <summary>새 디바이스 토큰을 등록합니다.</summary>
    /// <param name="userId">사용자 ID</param>
    /// <param name="token">FCM/APNs 토큰</param>
    /// <param name="platform">디바이스 플랫폼</param>
    public static Result<DeviceToken> Create(Guid userId, string token, DevicePlatform platform)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<DeviceToken>("사용자 ID는 필수입니다.");
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return Result.Failure<DeviceToken>("디바이스 토큰은 필수입니다.");
        }

        var deviceToken = new DeviceToken
        {
            UserId = userId,
            Token = token.Trim(),
            Platform = platform,
            IsActive = true,
        };

        return Result.Success(deviceToken);
    }

    /// <summary>토큰을 비활성화합니다 (로그아웃 또는 토큰 만료 시).</summary>
    public Result Deactivate()
    {
        if (!IsActive)
        {
            return Result.Failure("이미 비활성화된 토큰입니다.");
        }

        IsActive = false;
        return Result.Success();
    }
}
