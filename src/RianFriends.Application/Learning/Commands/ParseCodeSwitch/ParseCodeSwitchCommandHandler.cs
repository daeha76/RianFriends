using MediatR;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Common;
using RianFriends.Domain.Learning;

namespace RianFriends.Application.Learning.Commands.ParseCodeSwitch;

/// <summary>
/// Code-Switching 파싱 핸들러.
/// LLM 경량 모델로 파싱 후 Message.CodeSwitchData에 저장합니다.
/// </summary>
public sealed class ParseCodeSwitchCommandHandler : IRequestHandler<ParseCodeSwitchCommand, Result<CodeSwitchSegment[]>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ILlmService _llmService;
    private readonly ILogger<ParseCodeSwitchCommandHandler> _logger;

    private const string CodeSwitchSystemPrompt = """
        You are a multilingual parsing assistant.
        Given a text, extract all non-Korean foreign language segments.
        Return JSON array of objects: [{original, romanized, meaning, language}]
        language field uses BCP-47 codes (zh-CN, en, ja, etc.)
        If no foreign segments exist, return empty array [].
        """;

    /// <inheritdoc />
    public ParseCodeSwitchCommandHandler(
        IConversationRepository conversationRepository,
        ILlmService llmService,
        ILogger<ParseCodeSwitchCommandHandler> logger)
    {
        _conversationRepository = conversationRepository;
        _llmService = llmService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<CodeSwitchSegment[]>> Handle(ParseCodeSwitchCommand request, CancellationToken cancellationToken)
    {
        var message = await _conversationRepository.GetMessageByIdAsync(request.MessageId, cancellationToken);
        if (message is null)
        {
            return Result.Failure<CodeSwitchSegment[]>("메시지를 찾을 수 없습니다.");
        }

        try
        {
            // 경량 모델로 파싱 (Batch 모델 사용, 비용 절감)
            var promptMessage = Domain.Conversation.Message.Create(
                message.SessionId, "user",
                $"Parse this text: {request.MessageText}\nUser native language: {request.UserNativeLanguage}");

            if (promptMessage.IsFailure)
            {
                return Result.Failure<CodeSwitchSegment[]>(promptMessage.Error);
            }

            var json = await _llmService.GenerateResponseAsync(
                CodeSwitchSystemPrompt,
                [promptMessage.Value],
                [],
                cancellationToken);

            // JSON 파싱
            var segments = System.Text.Json.JsonSerializer.Deserialize<CodeSwitchSegment[]>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? [];

            // Message에 결과 저장
            message.SetCodeSwitchData(segments);
            await _conversationRepository.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("CodeSwitch 파싱 완료: MessageId={MessageId}, Segments={Count}", request.MessageId, segments.Length);
            return Result.Success(segments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CodeSwitch 파싱 실패: MessageId={MessageId}", request.MessageId);
            return Result.Failure<CodeSwitchSegment[]>("Code-Switching 파싱 중 오류가 발생했습니다.");
        }
    }
}
