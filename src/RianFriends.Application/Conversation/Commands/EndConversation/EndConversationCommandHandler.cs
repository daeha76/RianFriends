using MediatR;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Common;
using RianFriends.Domain.Friend;

namespace RianFriends.Application.Conversation.Commands.EndConversation;

/// <summary>대화 세션 종료 핸들러</summary>
public sealed class EndConversationCommandHandler : IRequestHandler<EndConversationCommand, Result>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IFriendRepository _friendRepository;
    private readonly IRedisContextService _redisContextService;
    private readonly ILogger<EndConversationCommandHandler> _logger;

    // 친구 성격별 기본 공감 게이지
    private static readonly Dictionary<PersonalityType, int> DefaultGaugeByPersonality = new()
    {
        { PersonalityType.Quiet, 70 },
        { PersonalityType.Energetic, 60 },
        { PersonalityType.Playful, 50 },
        { PersonalityType.Serious, 30 }
    };

    /// <inheritdoc />
    public EndConversationCommandHandler(
        IConversationRepository conversationRepository,
        IFriendRepository friendRepository,
        IRedisContextService redisContextService,
        ILogger<EndConversationCommandHandler> logger)
    {
        _conversationRepository = conversationRepository;
        _friendRepository = friendRepository;
        _redisContextService = redisContextService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(EndConversationCommand request, CancellationToken cancellationToken)
    {
        var session = await _conversationRepository.GetSessionByIdAsync(request.SessionId, cancellationToken);
        if (session is null || session.UserId != request.UserId)
        {
            return Result.Failure("유효하지 않은 세션입니다.");
        }

        // 친구 페르소나로 성격 기본 게이지 조회
        var friend = await _friendRepository.GetByIdAsync(session.FriendId, cancellationToken);
        var persona = friend is not null
            ? await _friendRepository.GetPersonaByIdAsync(friend.PersonaId, cancellationToken)
            : null;

        var defaultGauge = persona is not null && DefaultGaugeByPersonality.TryGetValue(persona.Personality, out var g) ? g : 60;

        var endResult = session.End(defaultGauge);
        if (endResult.IsFailure)
        {
            return endResult;
        }

        await _conversationRepository.SaveChangesAsync(cancellationToken);

        // Redis 컨텍스트 정리
        await _redisContextService.RemoveContextAsync(request.SessionId, cancellationToken);

        _logger.LogInformation("대화 세션 종료: SessionId={SessionId}", request.SessionId);
        return Result.Success();
    }
}
