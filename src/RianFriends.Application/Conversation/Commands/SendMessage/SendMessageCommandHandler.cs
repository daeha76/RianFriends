using MediatR;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Common;
using RianFriends.Domain.Conversation;
using RianFriends.Domain.Memory;

namespace RianFriends.Application.Conversation.Commands.SendMessage;

/// <summary>
/// 메시지 전송 핸들러.
/// 흐름: 세션 확인 → Redis 컨텍스트 조회 → 메모리 로드 → 시스템 프롬프트 구성 → LLM 스트리밍 → 저장.
/// 공감 모드에서는 CodeSwitch 이벤트가 발행되지 않습니다.
/// </summary>
public sealed class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result<IAsyncEnumerable<string>>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IFriendRepository _friendRepository;
    private readonly IMemoryRepository _memoryRepository;
    private readonly IRedisContextService _redisContextService;
    private readonly ILlmService _llmService;
    private readonly ILogger<SendMessageCommandHandler> _logger;

    /// <inheritdoc />
    public SendMessageCommandHandler(
        IConversationRepository conversationRepository,
        IFriendRepository friendRepository,
        IMemoryRepository memoryRepository,
        IRedisContextService redisContextService,
        ILlmService llmService,
        ILogger<SendMessageCommandHandler> logger)
    {
        _conversationRepository = conversationRepository;
        _friendRepository = friendRepository;
        _memoryRepository = memoryRepository;
        _redisContextService = redisContextService;
        _llmService = llmService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<IAsyncEnumerable<string>>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        // 1. 세션 & 권한 검증
        var session = await _conversationRepository.GetSessionByIdAsync(request.SessionId, cancellationToken);
        if (session is null || session.UserId != request.UserId)
        {
            return Result.Failure<IAsyncEnumerable<string>>("유효하지 않은 세션입니다.");
        }

        if (!session.IsActive)
        {
            return Result.Failure<IAsyncEnumerable<string>>("종료된 세션입니다.");
        }

        // 2. 친구 & 페르소나 로드
        var friend = await _friendRepository.GetByIdAsync(session.FriendId, cancellationToken);
        if (friend is null)
        {
            return Result.Failure<IAsyncEnumerable<string>>("친구를 찾을 수 없습니다.");
        }

        var persona = await _friendRepository.GetPersonaByIdAsync(friend.PersonaId, cancellationToken);
        if (persona is null)
        {
            return Result.Failure<IAsyncEnumerable<string>>("페르소나를 찾을 수 없습니다.");
        }

        // 3. 사용자 메시지 저장
        var userMessageResult = Message.Create(request.SessionId, "user", request.Content);
        if (userMessageResult.IsFailure)
        {
            return Result.Failure<IAsyncEnumerable<string>>(userMessageResult.Error);
        }

        _conversationRepository.AddMessage(userMessageResult.Value);
        await _conversationRepository.SaveChangesAsync(cancellationToken);

        // 4. Redis 컨텍스트 + DB 메모리 조회 (ShortTerm + MidTerm)
        var contextMessages = await _redisContextService.GetContextAsync(request.SessionId, cancellationToken);
        if (contextMessages.Count == 0)
        {
            contextMessages = await _conversationRepository.GetRecentMessagesAsync(request.SessionId, 20, cancellationToken);
        }
        var memories = (await _memoryRepository.GetContextMemoriesAsync(session.FriendId, cancellationToken)).ToArray();

        // 5. 시스템 프롬프트 구성 (페르소나 + 공감 게이지)
        var systemPrompt = persona.BuildSystemPromptSection()
                         + session.EmpathySettings.BuildEmpathyPromptSection();

        // 6. LLM SSE 스트리밍 시작
        _logger.LogInformation("LLM 스트리밍 시작: SessionId={SessionId}, Mode={Mode}", request.SessionId, session.Mode);
        var stream = _llmService.StreamResponseAsync(systemPrompt, [.. contextMessages], memories, cancellationToken);

        // 7. 스트리밍 결과를 래핑하여 반환 (Controller에서 SSE로 전달 후 DB 저장)
        return Result.Success(stream);
    }
}
