using FluentValidation;

namespace RianFriends.Application.Conversation.Commands.SetEmpathyGauge;

/// <summary>공감 게이지 설정 유효성 검증기</summary>
public sealed class SetEmpathyGaugeCommandValidator : AbstractValidator<SetEmpathyGaugeCommand>
{
    /// <inheritdoc />
    public SetEmpathyGaugeCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty().WithMessage("세션 ID는 필수입니다.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("사용자 ID는 필수입니다.");
        RuleFor(x => x.Gauge)
            .InclusiveBetween(0, 100).WithMessage("게이지는 0~100 범위여야 합니다.");
        RuleFor(x => x.ControlMode).IsInEnum().WithMessage("유효하지 않은 제어 방식입니다.");
    }
}
