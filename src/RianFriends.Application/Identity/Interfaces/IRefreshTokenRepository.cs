using RianFriends.Domain.Identity;

namespace RianFriends.Application.Identity.Interfaces;

/// <summary>RefreshToken 저장소 인터페이스</summary>
public interface IRefreshTokenRepository
{
    /// <summary>토큰 해시로 활성 RefreshToken을 조회합니다.</summary>
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>RefreshToken을 추가합니다.</summary>
    void Add(RefreshToken refreshToken);

    /// <summary>사용자의 모든 활성 RefreshToken을 폐기합니다.</summary>
    Task RevokeAllByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>변경사항을 저장합니다.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
