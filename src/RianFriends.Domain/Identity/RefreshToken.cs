using RianFriends.Domain.Common;

namespace RianFriends.Domain.Identity;

/// <summary>
/// RefreshToken 엔티티. DB에 저장하여 토큰 회전(rotation) 및 만료 관리를 담당합니다.
/// 비유: 도서관 대출증 — 분실 신고(Revoke)하면 이전 카드로는 대출 불가.
/// </summary>
public sealed class RefreshToken : BaseEntity
{
    /// <summary>토큰 소유 사용자 ID</summary>
    public Guid UserId { get; private set; }

    /// <summary>토큰 문자열 (SHA256 해시로 저장)</summary>
    public string TokenHash { get; private set; } = string.Empty;

    /// <summary>만료 일시 (UTC)</summary>
    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>발급 일시 (UTC)</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>폐기 일시 (null이면 유효)</summary>
    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>교체된 새 토큰 ID (rotation 추적)</summary>
    public Guid? ReplacedByTokenId { get; private set; }

    /// <summary>토큰이 아직 유효한지 여부</summary>
    public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;

    /// <summary>EF Core용 기본 생성자</summary>
    private RefreshToken() { }

    /// <summary>새 RefreshToken을 생성합니다.</summary>
    public static RefreshToken Create(Guid userId, string tokenHash, int expirationDays)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(expirationDays)
        };
    }

    /// <summary>토큰을 폐기하고 교체 토큰을 기록합니다 (Rotation).</summary>
    public void Revoke(Guid? replacedByTokenId = null)
    {
        RevokedAt = DateTimeOffset.UtcNow;
        ReplacedByTokenId = replacedByTokenId;
    }
}
