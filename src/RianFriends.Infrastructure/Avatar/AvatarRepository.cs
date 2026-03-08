using Microsoft.EntityFrameworkCore;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Avatar;
using RianFriends.Infrastructure.Persistence;

namespace RianFriends.Infrastructure.Avatar;

/// <summary>IAvatarRepository 구현체</summary>
internal sealed class AvatarRepository : IAvatarRepository
{
    private readonly AppDbContext _context;

    /// <inheritdoc />
    public AvatarRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public Task<Domain.Avatar.Avatar?> GetByFriendIdAsync(Guid friendId, CancellationToken ct)
    {
        return _context.Avatars
            .FirstOrDefaultAsync(a => a.FriendId == friendId, ct);
    }

    /// <inheritdoc />
    public void Add(Domain.Avatar.Avatar avatar)
    {
        _context.Avatars.Add(avatar);
    }

    /// <inheritdoc />
    public void AddSnack(Snack snack)
    {
        _context.Snacks.Add(snack);
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return _context.SaveChangesAsync(ct);
    }
}
