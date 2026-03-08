using Microsoft.EntityFrameworkCore;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Billing;
using RianFriends.Infrastructure.Persistence;

namespace RianFriends.Infrastructure.Billing;

/// <summary>IUserQuotaRepository 구현체</summary>
internal sealed class UserQuotaRepository : IUserQuotaRepository
{
    private readonly AppDbContext _context;

    /// <inheritdoc />
    public UserQuotaRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public Task<UserQuota?> GetTodayAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return _context.UserQuotas
            .FirstOrDefaultAsync(q => q.UserId == userId && q.Date == today, cancellationToken);
    }

    /// <inheritdoc />
    public void Add(UserQuota quota)
    {
        _context.UserQuotas.Add(quota);
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task ResetAllAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // 어제 날짜의 쿼터를 오늘로 갱신하고 UsedTokens 초기화
        // EF Core ExecuteUpdateAsync 사용 (EF 7+ bulk update)
        await _context.UserQuotas
            .Where(q => q.Date < today)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(q => q.UsedTokens, 0)
                .SetProperty(q => q.Date, today),
                cancellationToken);
    }
}
