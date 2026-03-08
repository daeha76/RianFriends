using FluentValidation;

namespace RianFriends.Application.Identity.Commands.RefreshToken;

/// <summary>RefreshTokenCommand 유효성 검증</summary>
public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    /// <summary>유효성 규칙을 설정합니다.</summary>
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("RefreshToken은 필수입니다.");
    }
}
