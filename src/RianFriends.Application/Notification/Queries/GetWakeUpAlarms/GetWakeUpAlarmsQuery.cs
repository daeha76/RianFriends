using RianFriends.Application.Abstractions;
using RianFriends.Application.Notification.Dtos;

namespace RianFriends.Application.Notification.Queries.GetWakeUpAlarms;

/// <summary>사용자의 기상 알람 목록 조회 쿼리</summary>
public sealed record GetWakeUpAlarmsQuery(Guid UserId) : IQuery<List<WakeUpAlarmDto>>;
