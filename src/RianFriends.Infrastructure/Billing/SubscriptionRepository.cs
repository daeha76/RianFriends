using Microsoft.EntityFrameworkCore;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Billing;
using RianFriends.Infrastructure.Persistence;

namespace RianFriends.Infrastructure.Billing;

/// <summary>ISubscriptionRepository 구현체</summary>
internal sealed class SubscriptionRepository : ISubscriptionRepository
{
    private readonly AppDbContext _context;

    /// <inheritdoc />
    public SubscriptionRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public Task<Subscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive, cancellationToken);
    }

    /// <inheritdoc />
    public void Add(Subscription subscription)
    {
        _context.Subscriptions.Add(subscription);
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
