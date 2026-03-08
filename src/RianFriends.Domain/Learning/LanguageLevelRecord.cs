using RianFriends.Domain.Common;

namespace RianFriends.Domain.Learning;

/// <summary>
/// 사용자의 언어 레벨 평가 기록.
/// 언어별로 독립 저장 (중국어 Middle, 영어 Elementary 가능).
/// 10회 세션마다 배치 잡에서 재평가합니다.
/// </summary>
public sealed class LanguageLevelRecord : AuditableEntity
{
    /// <summary>평가 대상 사용자</summary>
    public Guid UserId { get; private set; }

    /// <summary>평가 대상 친구 (친구별로 레벨이 다를 수 있음)</summary>
    public Guid FriendId { get; private set; }

    /// <summary>평가 언어 코드 (예: "zh-CN", "en", "ja")</summary>
    public string Language { get; private set; } = string.Empty;

    /// <summary>현재 판정된 언어 레벨</summary>
    public LanguageLevel Level { get; private set; }

    /// <summary>마지막 평가 일시 (UTC)</summary>
    public DateTimeOffset EvaluatedAt { get; private set; }

    /// <summary>EF Core용 기본 생성자</summary>
    private LanguageLevelRecord() { }

    /// <summary>새 언어 레벨 기록을 생성합니다.</summary>
    public static Result<LanguageLevelRecord> Create(Guid userId, Guid friendId, string language, LanguageLevel level)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<LanguageLevelRecord>("사용자 ID는 필수입니다.");
        }

        if (string.IsNullOrWhiteSpace(language))
        {
            return Result.Failure<LanguageLevelRecord>("언어 코드는 필수입니다.");
        }

        var record = new LanguageLevelRecord
        {
            UserId = userId,
            FriendId = friendId,
            Language = language,
            Level = level,
            EvaluatedAt = DateTimeOffset.UtcNow
        };

        return Result.Success(record);
    }

    /// <summary>레벨을 업데이트합니다 (재평가 시 호출).</summary>
    public void UpdateLevel(LanguageLevel newLevel)
    {
        Level = newLevel;
        EvaluatedAt = DateTimeOffset.UtcNow;
    }
}
