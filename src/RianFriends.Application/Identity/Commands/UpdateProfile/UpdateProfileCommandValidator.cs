using FluentValidation;

namespace RianFriends.Application.Identity.Commands.UpdateProfile;

/// <summary>UpdateProfileCommand 유효성 검사</summary>
public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    /// <summary>유효성 검사 규칙을 초기화합니다.</summary>
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.Nickname)
            .NotEmpty().WithMessage("닉네임은 필수입니다.")
            .MinimumLength(2).WithMessage("닉네임은 최소 2자 이상이어야 합니다.")
            .MaximumLength(20).WithMessage("닉네임은 최대 20자까지 입력할 수 있습니다.");

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("생년월일은 필수입니다.")
            .Must(b => b <= DateOnly.FromDateTime(DateTime.Today.AddYears(-13)))
            .WithMessage("만 13세 미만은 서비스를 이용할 수 없습니다.");

        RuleFor(x => x.CountryCode)
            .NotEmpty().WithMessage("국가 코드는 필수입니다.")
            .Length(2).WithMessage("국가 코드는 ISO 3166-1 alpha-2 형식이어야 합니다 (예: KR, US).");
    }
}
