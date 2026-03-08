using RianFriends.Application.Abstractions;

namespace RianFriends.Application.Notification.Commands.DeleteWakeUpAlarm;

/// <summary>기상 알람 삭제 커맨드</summary>
public sealed record DeleteWakeUpAlarmCommand(Guid UserId, Guid AlarmId) : ICommand;
