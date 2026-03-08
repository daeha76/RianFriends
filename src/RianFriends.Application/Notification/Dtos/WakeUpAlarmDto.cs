namespace RianFriends.Application.Notification.Dtos;

/// <summary>기상 알람 DTO</summary>
public sealed record WakeUpAlarmDto(
    Guid AlarmId,
    Guid FriendId,
    TimeOnly AlarmTime,
    bool IsEnabled,
    byte RepeatDays);
