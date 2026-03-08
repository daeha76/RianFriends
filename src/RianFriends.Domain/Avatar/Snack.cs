using RianFriends.Domain.Common;

namespace RianFriends.Domain.Avatar;

/// <summary>
/// 아바타에게 준 간식 기록 엔티티.
/// 먹이 이력을 보관하기 위해 별도 테이블에 저장합니다.
/// </summary>
public sealed class Snack : AuditableEntity
{
    /// <summary>간식을 받은 아바타 ID</summary>
    public Guid AvatarId { get; private set; }

    /// <summary>간식 종류 (예: "cookie", "sandwich")</summary>
    public string SnackType { get; private set; } = string.Empty;

    /// <summary>먹인 시각</summary>
    public DateTimeOffset FedAt { get; private set; }

    /// <summary>EF Core용 기본 생성자</summary>
    private Snack() { }

    /// <summary>새 간식 기록을 생성합니다.</summary>
    /// <param name="avatarId">아바타 ID</param>
    /// <param name="snackType">간식 종류</param>
    public static Result<Snack> Create(Guid avatarId, string snackType)
    {
        if (avatarId == Guid.Empty)
        {
            return Result.Failure<Snack>("아바타 ID는 필수입니다.");
        }

        if (string.IsNullOrWhiteSpace(snackType))
        {
            return Result.Failure<Snack>("간식 종류는 필수입니다.");
        }

        var snack = new Snack
        {
            AvatarId = avatarId,
            SnackType = snackType.Trim(),
            FedAt = DateTimeOffset.UtcNow,
        };

        return Result.Success(snack);
    }
}
