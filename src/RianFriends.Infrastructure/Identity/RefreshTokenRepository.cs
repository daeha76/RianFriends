using Microsoft.EntityFrameworkCore;
using RianFriends.Application.Identity.Interfaces;
using RianFriends.Domain.Identity;
using RianFriends.Infrastructure.Persistence;

namespace RianFriends.Infrastructure.Identity;

/// <summary>RefreshToken 저장소 구현체</summary>
internal sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    /// <summary>의존성을 주입합니다.</summary>
    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
        => await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);

    /// <inheritdoc />
    public void Add(RefreshToken refreshToken)
        => _context.RefreshTokens.Add(refreshToken);

    /// <inheritdoc />
    public async Task RevokeAllByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in activeTokens)
        {
            token.Revoke();
        }
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
