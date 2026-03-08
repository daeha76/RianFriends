using RianFriends.Domain.Common;
using RianFriends.Domain.Conversation.Events;

namespace RianFriends.Domain.Conversation;

/// <summary>
/// AI 친구와의 대화 세션. 공감 모드 등 세션 상태를 관리합니다.
/// 세션 종료 시 Language 모드로 자동 복귀하고, 공감 게이지는 성격 기본값으로 초기화됩니다.
/// </summary>
public sealed class ConversationSession : AuditableEntity
{
    /// <summary>이 세션에 참여하는 AI 친구 ID</summary>
    public Guid FriendId { get; private set; }

    /// <summary>이 세션을 시작한 사용자 ID</summary>
    public Guid UserId { get; private set; }

    /// <summary>현재 대화 모드 (Language / Empathy)</summary>
    public ConversationMode Mode => EmpathySettings.Mode;

    /// <summary>공감 모드 설정</summary>
    public EmpathySettings EmpathySettings { get; private set; } = new();

    /// <summary>이 친구와 몇 번째 세션인지 (언어 레벨 평가 기준: 10회마다)</summary>
    public int SessionNumber { get; private set; }

    /// <summary>세션 종료 시각 (null이면 진행 중)</summary>
    public DateTimeOffset? EndedAt { get; private set; }

    /// <summary>세션이 진행 중인지 여부</summary>
    public bool IsActive => EndedAt is null;

    /// <summary>EF Core용 기본 생성자</summary>
    private ConversationSession() { }

    /// <summary>새 대화 세션을 시작합니다.</summary>
    public static Result<ConversationSession> Start(Guid userId, Guid friendId, int sessionNumber)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<ConversationSession>("사용자 ID는 필수입니다.");
        }

        if (friendId == Guid.Empty)
        {
            return Result.Failure<ConversationSession>("친구 ID는 필수입니다.");
        }

        var session = new ConversationSession
        {
            UserId = userId,
            FriendId = friendId,
            SessionNumber = sessionNumber,
            EmpathySettings = new EmpathySettings()
        };

        return Result.Success(session);
    }

    /// <summary>공감 게이지를 설정합니다.</summary>
    public void SetEmpathyGauge(int gauge, GaugeControlMode source)
        => EmpathySettings.SetGauge(gauge, source);

    /// <summary>
    /// 세션을 종료합니다.
    /// Language 모드로 복귀, 공감 게이지는 성격 기본값으로 초기화됩니다.
    /// </summary>
    public Result End(int personalityDefaultGauge)
    {
        if (!IsActive)
        {
            return Result.Failure("이미 종료된 세션입니다.");
        }

        EndedAt = DateTimeOffset.UtcNow;
        EmpathySettings.ResetToPersonalityDefault(personalityDefaultGauge);
        AddDomainEvent(new SessionEndedEvent(Id, UserId, FriendId, SessionNumber));

        return Result.Success();
    }
}
