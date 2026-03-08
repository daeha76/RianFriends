using Microsoft.EntityFrameworkCore;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Memory;
using RianFriends.Infrastructure.Persistence;

namespace RianFriends.Infrastructure.Memory;

internal sealed class MemoryRepository : IMemoryRepository
{
    private readonly AppDbContext _context;

    public MemoryRepository(AppDbContext context) => _context = context;

    public Task<List<FriendMemory>> GetContextMemoriesAsync(Guid friendId, CancellationToken ct = default)
        => _context.FriendMemories
            .Where(m => m.FriendId == friendId
                     && (m.Layer == MemoryLayer.ShortTerm || m.Layer == MemoryLayer.MidTerm)
                     && m.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderBy(m => m.Layer)
            .ThenByDescending(m => m.CreatedAt)
            .ToListAsync(ct);

    public Task<List<FriendMemory>> GetExpiredMemoriesAsync(MemoryLayer layer, CancellationToken ct = default)
        => _context.FriendMemories
            .Where(m => m.Layer == layer && m.ExpiresAt <= DateTimeOffset.UtcNow)
            .ToListAsync(ct);

    public void Add(FriendMemory memory) => _context.FriendMemories.Add(memory);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
