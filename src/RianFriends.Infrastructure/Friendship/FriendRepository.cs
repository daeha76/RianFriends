using Microsoft.EntityFrameworkCore;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Friend;
using RianFriends.Infrastructure.Persistence;

namespace RianFriends.Infrastructure.Friend;

internal sealed class FriendRepository : IFriendRepository
{
    private readonly AppDbContext _context;

    public FriendRepository(AppDbContext context) => _context = context;

    public Task<Domain.Friend.Friend?> GetByIdAsync(Guid friendId, CancellationToken ct = default)
        => _context.Friends.FirstOrDefaultAsync(f => f.Id == friendId, ct);

    public Task<List<Domain.Friend.Friend>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => _context.Friends
            .Where(f => f.UserId == userId && f.IsActive)
            .OrderBy(f => f.CreatedAt)
            .ToListAsync(ct);

    public Task<int> CountActiveByUserIdAsync(Guid userId, CancellationToken ct = default)
        => _context.Friends.CountAsync(f => f.UserId == userId && f.IsActive, ct);

    public Task<FriendPersona?> GetPersonaByIdAsync(Guid personaId, CancellationToken ct = default)
        => _context.FriendPersonas.FirstOrDefaultAsync(p => p.Id == personaId, ct);

    public Task<List<FriendPersona>> GetAllPersonasAsync(CancellationToken ct = default)
        => _context.FriendPersonas.OrderBy(p => p.Name).ToListAsync(ct);

    public void Add(Domain.Friend.Friend friend) => _context.Friends.Add(friend);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
