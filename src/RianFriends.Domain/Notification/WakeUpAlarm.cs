using RianFriends.Domain.Common;

namespace RianFriends.Domain.Notification;

/// <summary>
/// 기상 알람 엔티티.
/// 사용자가 특정 시간에 AI 친구의 목소리로 알람이 울리도록 설정합니다.
/// RepeatDays는 비트 마스크로 요일을 표현합니다 (bit 0=일, 1=월, ..., 6=토).
/// </summary>
public sealed class WakeUpAlarm : AuditableEntity
{
    /// <summary>알람 소유 사용자 ID</summary>
    public Guid UserId { get; private set; }

    /// <summary>알람에 사용될 AI 친구 ID</summary>
    public Guid FriendId { get; private set; }

    /// <summary>알람 시각 (시:분)</summary>
    public TimeOnly AlarmTime { get; private set; }

    /// <summary>알람 활성 여부</summary>
    public bool IsEnabled { get; private set; } = true;

    /// <summary>
    /// 반복 요일 비트 마스크 (bit 0=일, 1=월, 2=화, 3=수, 4=목, 5=금, 6=토).
    /// 0이면 일회성 알람.
    /// </summary>
    public byte RepeatDays { get; private set; }

    /// <summary>EF Core용 기본 생성자</summary>
    private WakeUpAlarm() { }

    /// <summary>새 기상 알람을 생성합니다.</summary>
    /// <param name="userId">사용자 ID</param>
    /// <param name="friendId">알람에 사용할 AI 친구 ID</param>
    /// <param name="alarmTime">알람 시각</param>
    /// <param name="repeatDays">반복 요일 비트 마스크 (0=일회성)</param>
    public static Result<WakeUpAlarm> Create(Guid userId, Guid friendId, TimeOnly alarmTime, byte repeatDays = 0)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<WakeUpAlarm>("사용자 ID는 필수입니다.");
        }

        if (friendId == Guid.Empty)
        {
            return Result.Failure<WakeUpAlarm>("친구 ID는 필수입니다.");
        }

        var alarm = new WakeUpAlarm
        {
            UserId = userId,
            FriendId = friendId,
            AlarmTime = alarmTime,
            RepeatDays = repeatDays,
            IsEnabled = true,
        };

        return Result.Success(alarm);
    }

    /// <summary>알람 시각 및 반복 요일을 수정합니다.</summary>
    /// <param name="alarmTime">새 알람 시각</param>
    /// <param name="repeatDays">새 반복 요일 비트 마스크</param>
    public Result UpdateAlarm(TimeOnly alarmTime, byte repeatDays)
    {
        AlarmTime = alarmTime;
        RepeatDays = repeatDays;
        return Result.Success();
    }

    /// <summary>알람 활성/비활성 상태를 전환합니다.</summary>
    public Result Toggle()
    {
        IsEnabled = !IsEnabled;
        return Result.Success();
    }
}
