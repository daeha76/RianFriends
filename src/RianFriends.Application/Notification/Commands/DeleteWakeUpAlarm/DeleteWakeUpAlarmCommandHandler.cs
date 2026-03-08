using MediatR;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Notification.Commands.DeleteWakeUpAlarm;

/// <summary>기상 알람 삭제 핸들러</summary>
public sealed class DeleteWakeUpAlarmCommandHandler : IRequestHandler<DeleteWakeUpAlarmCommand, Result>
{
    private readonly IAlarmRepository _alarmRepository;
    private readonly ILogger<DeleteWakeUpAlarmCommandHandler> _logger;

    /// <inheritdoc />
    public DeleteWakeUpAlarmCommandHandler(
        IAlarmRepository alarmRepository,
        ILogger<DeleteWakeUpAlarmCommandHandler> logger)
    {
        _alarmRepository = alarmRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeleteWakeUpAlarmCommand request, CancellationToken cancellationToken)
    {
        var alarm = await _alarmRepository.GetByIdAsync(request.AlarmId, request.UserId, cancellationToken);

        if (alarm is null)
        {
            return Result.Failure("알람을 찾을 수 없거나 접근 권한이 없습니다.");
        }

        _alarmRepository.Remove(alarm);
        await _alarmRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "기상 알람 삭제: UserId={UserId}, AlarmId={AlarmId}",
            request.UserId,
            request.AlarmId);

        return Result.Success();
    }
}
