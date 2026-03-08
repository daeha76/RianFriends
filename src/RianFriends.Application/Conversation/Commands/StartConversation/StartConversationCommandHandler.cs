using MediatR;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Common;
using RianFriends.Domain.Conversation;

namespace RianFriends.Application.Conversation.Commands.StartConversation;

/// <summary>대화 세션 시작 핸들러</summary>
public sealed class StartConversationCommandHandler : IRequestHandler<StartConversationCommand, Result<Guid>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IFriendRepository _friendRepository;
    private readonly ILogger<StartConversationCommandHandler> _logger;

    /// <inheritdoc />
    public StartConversationCommandHandler(
        IConversationRepository conversationRepository,
        IFriendRepository friendRepository,
        ILogger<StartConversationCommandHandler> logger)
    {
        _conversationRepository = conversationRepository;
        _friendRepository = friendRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(StartConversationCommand request, CancellationToken cancellationToken)
    {
        // 1. 친구 존재 & 권한 확인
        var friend = await _friendRepository.GetByIdAsync(request.FriendId, cancellationToken);
        if (friend is null || friend.UserId != request.UserId)
        {
            return Result.Failure<Guid>("유효하지 않은 친구입니다.");
        }

        if (!friend.IsActive)
        {
            return Result.Failure<Guid>("비활성화된 친구와는 대화할 수 없습니다.");
        }

        // 2. 세션 번호 계산 (언어 레벨 평가 주기 기준)
        var sessionCount = await _conversationRepository.CountSessionsByFriendIdAsync(request.FriendId, cancellationToken);
        var sessionNumber = sessionCount + 1;

        // 3. 세션 생성
        var sessionResult = ConversationSession.Start(request.UserId, request.FriendId, sessionNumber);
        if (sessionResult.IsFailure)
        {
            return Result.Failure<Guid>(sessionResult.Error);
        }

        _conversationRepository.AddSession(sessionResult.Value);
        await _conversationRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("대화 세션 시작: SessionId={SessionId}, FriendId={FriendId}", sessionResult.Value.Id, request.FriendId);
        return Result.Success(sessionResult.Value.Id);
    }
}
