using MediatR;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Learning.Commands.EvaluateLanguageLevel;

/// <summary>
/// 언어 레벨 재평가 명령 (배치 처리 전용).
/// ConversationSession 10회마다 배치 잡에서 호출합니다.
/// 실시간 대화 중에는 절대 호출하지 않습니다.
/// </summary>
public record EvaluateLanguageLevelCommand(
    Guid UserId,
    Guid FriendId,
    string Language) : IRequest<Result>;
