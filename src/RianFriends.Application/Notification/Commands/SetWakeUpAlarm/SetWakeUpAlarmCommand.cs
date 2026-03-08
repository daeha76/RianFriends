using RianFriends.Application.Abstractions;

namespace RianFriends.Application.Notification.Commands.SetWakeUpAlarm;

/// <summary>
/// 기상 알람 생성 또는 수정 커맨드.
/// 새 알람을 등록하고 알람 ID를 반환합니다.
/// </summary>
public sealed record SetWakeUpAlarmCommand(
    Guid UserId,
    Guid FriendId,
    TimeOnly AlarmTime,
    byte RepeatDays = 0) : ICommand<Guid>;
