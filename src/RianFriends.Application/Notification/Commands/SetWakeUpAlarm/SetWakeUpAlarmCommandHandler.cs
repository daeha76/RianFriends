using MediatR;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Common;
using RianFriends.Domain.Notification;

namespace RianFriends.Application.Notification.Commands.SetWakeUpAlarm;

/// <summary>기상 알람 생성 핸들러</summary>
public sealed class SetWakeUpAlarmCommandHandler : IRequestHandler<SetWakeUpAlarmCommand, Result<Guid>>
{
    private readonly IAlarmRepository _alarmRepository;
    private readonly ILogger<SetWakeUpAlarmCommandHandler> _logger;

    /// <inheritdoc />
    public SetWakeUpAlarmCommandHandler(
        IAlarmRepository alarmRepository,
        ILogger<SetWakeUpAlarmCommandHandler> logger)
    {
        _alarmRepository = alarmRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(SetWakeUpAlarmCommand request, CancellationToken cancellationToken)
    {
        var alarmResult = WakeUpAlarm.Create(
            request.UserId,
            request.FriendId,
            request.AlarmTime,
            request.RepeatDays);

        if (alarmResult.IsFailure)
        {
            return Result.Failure<Guid>(alarmResult.Error);
        }

        _alarmRepository.Add(alarmResult.Value);
        await _alarmRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "기상 알람 등록: UserId={UserId}, AlarmId={AlarmId}, AlarmTime={AlarmTime}",
            request.UserId,
            alarmResult.Value.Id,
            request.AlarmTime);

        return Result.Success(alarmResult.Value.Id);
    }
}
