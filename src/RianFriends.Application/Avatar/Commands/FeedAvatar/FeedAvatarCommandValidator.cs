using FluentValidation;

namespace RianFriends.Application.Avatar.Commands.FeedAvatar;

/// <summary>FeedAvatarCommand 유효성 검증기</summary>
public sealed class FeedAvatarCommandValidator : AbstractValidator<FeedAvatarCommand>
{
    /// <inheritdoc />
    public FeedAvatarCommandValidator()
    {
        RuleFor(x => x.FriendId)
            .NotEmpty().WithMessage("친구 ID는 필수입니다.");

        RuleFor(x => x.SnackType)
            .NotEmpty().WithMessage("간식 종류는 필수입니다.")
            .MaximumLength(50).WithMessage("간식 종류는 50자 이하여야 합니다.");
    }
}
