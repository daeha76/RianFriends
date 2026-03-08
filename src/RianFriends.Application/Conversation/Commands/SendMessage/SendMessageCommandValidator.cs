using FluentValidation;

namespace RianFriends.Application.Conversation.Commands.SendMessage;

/// <summary>메시지 전송 유효성 검증기</summary>
public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    /// <inheritdoc />
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty().WithMessage("세션 ID는 필수입니다.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("사용자 ID는 필수입니다.");
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("메시지 내용은 필수입니다.")
            .MaximumLength(2000).WithMessage("메시지는 2000자를 초과할 수 없습니다.");
    }
}
