using FluentValidation;

namespace RianFriends.Application.Notification.Commands.SetWakeUpAlarm;

/// <summary>SetWakeUpAlarmCommand 유효성 검증기</summary>
public sealed class SetWakeUpAlarmCommandValidator : AbstractValidator<SetWakeUpAlarmCommand>
{
    /// <inheritdoc />
    public SetWakeUpAlarmCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("사용자 ID는 필수입니다.");

        RuleFor(x => x.FriendId)
            .NotEmpty().WithMessage("친구 ID는 필수입니다.");

        RuleFor(x => x.RepeatDays)
            .InclusiveBetween((byte)0, (byte)127)
            .WithMessage("반복 요일 값은 0–127 사이여야 합니다.");
    }
}
