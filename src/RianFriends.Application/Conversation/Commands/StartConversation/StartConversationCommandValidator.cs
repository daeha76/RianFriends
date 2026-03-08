using FluentValidation;

namespace RianFriends.Application.Conversation.Commands.StartConversation;

/// <summary>대화 세션 시작 유효성 검증기</summary>
public sealed class StartConversationCommandValidator : AbstractValidator<StartConversationCommand>
{
    /// <inheritdoc />
    public StartConversationCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("사용자 ID는 필수입니다.");
        RuleFor(x => x.FriendId).NotEmpty().WithMessage("친구 ID는 필수입니다.");
    }
}
