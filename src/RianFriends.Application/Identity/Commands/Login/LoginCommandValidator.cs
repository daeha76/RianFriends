using FluentValidation;

namespace RianFriends.Application.Identity.Commands.Login;

/// <summary>LoginCommand 유효성 검증</summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    private static readonly string[] AllowedProviders = ["google", "naver", "kakao", "apple", "email"];

    /// <summary>유효성 규칙을 설정합니다.</summary>
    public LoginCommandValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("프로바이더는 필수입니다.")
            .Must(p => AllowedProviders.Contains(p))
            .WithMessage($"지원하는 프로바이더: {string.Join(", ", AllowedProviders)}");

        RuleFor(x => x.Credential)
            .NotEmpty().WithMessage("인증 정보는 필수입니다.");

        When(x => x.Provider == "email", () =>
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("이메일 로그인 시 이메일은 필수입니다.")
                .EmailAddress().WithMessage("올바른 이메일 형식이 아닙니다.");
        });
    }
}
