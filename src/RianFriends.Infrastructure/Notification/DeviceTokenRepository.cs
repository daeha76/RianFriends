using Microsoft.EntityFrameworkCore;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Notification;
using RianFriends.Infrastructure.Persistence;

namespace RianFriends.Infrastructure.Notification;

/// <summary>IDeviceTokenRepository 구현체</summary>
internal sealed class DeviceTokenRepository : IDeviceTokenRepository
{
    private readonly AppDbContext _context;

    /// <inheritdoc />
    public DeviceTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public Task<List<DeviceToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken ct)
    {
        return _context.DeviceTokens
            .Where(d => d.UserId == userId && d.IsActive)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public void Add(DeviceToken deviceToken)
    {
        _context.DeviceTokens.Add(deviceToken);
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return _context.SaveChangesAsync(ct);
    }
}
