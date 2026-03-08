using MediatR;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Conversation.Commands.SetEmpathyGauge;

/// <summary>공감 게이지 설정 핸들러</summary>
public sealed class SetEmpathyGaugeCommandHandler : IRequestHandler<SetEmpathyGaugeCommand, Result>
{
    private readonly IConversationRepository _conversationRepository;

    /// <inheritdoc />
    public SetEmpathyGaugeCommandHandler(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(SetEmpathyGaugeCommand request, CancellationToken cancellationToken)
    {
        var session = await _conversationRepository.GetSessionByIdAsync(request.SessionId, cancellationToken);
        if (session is null || session.UserId != request.UserId)
        {
            return Result.Failure("유효하지 않은 세션입니다.");
        }

        if (!session.IsActive)
        {
            return Result.Failure("종료된 세션입니다.");
        }

        try
        {
            session.SetEmpathyGauge(request.Gauge, request.ControlMode);
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        await _conversationRepository.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
