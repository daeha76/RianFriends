using FluentValidation;

namespace RianFriends.Application.Notification.Commands.RegisterDeviceToken;

/// <summary>RegisterDeviceTokenCommand 유효성 검증기</summary>
public sealed class RegisterDeviceTokenCommandValidator : AbstractValidator<RegisterDeviceTokenCommand>
{
    /// <inheritdoc />
    public RegisterDeviceTokenCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("사용자 ID는 필수입니다.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("디바이스 토큰은 필수입니다.")
            .MaximumLength(500).WithMessage("토큰은 500자 이하여야 합니다.");
    }
}
