using MediatR;
using RianFriends.Application.Abstractions;
using RianFriends.Application.Notification.Dtos;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Notification.Queries.GetWakeUpAlarms;

/// <summary>기상 알람 목록 조회 핸들러</summary>
public sealed class GetWakeUpAlarmsQueryHandler : IRequestHandler<GetWakeUpAlarmsQuery, Result<List<WakeUpAlarmDto>>>
{
    private readonly IAlarmRepository _alarmRepository;

    /// <inheritdoc />
    public GetWakeUpAlarmsQueryHandler(IAlarmRepository alarmRepository)
    {
        _alarmRepository = alarmRepository;
    }

    /// <inheritdoc />
    public async Task<Result<List<WakeUpAlarmDto>>> Handle(GetWakeUpAlarmsQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            return Result.Failure<List<WakeUpAlarmDto>>("사용자 ID는 필수입니다.");
        }

        var alarms = await _alarmRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        var dtos = alarms.Select(a => new WakeUpAlarmDto(
            a.Id,
            a.FriendId,
            a.AlarmTime,
            a.IsEnabled,
            a.RepeatDays)).ToList();

        return Result.Success(dtos);
    }
}
