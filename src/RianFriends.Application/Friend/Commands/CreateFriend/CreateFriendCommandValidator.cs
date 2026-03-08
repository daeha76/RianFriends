using FluentValidation;

namespace RianFriends.Application.Friend.Commands.CreateFriend;

/// <summary>AI 친구 생성 유효성 검증기</summary>
public sealed class CreateFriendCommandValidator : AbstractValidator<CreateFriendCommand>
{
    /// <inheritdoc />
    public CreateFriendCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("사용자 ID는 필수입니다.");
        RuleFor(x => x.PersonaId).NotEmpty().WithMessage("페르소나 ID는 필수입니다.");
    }
}
