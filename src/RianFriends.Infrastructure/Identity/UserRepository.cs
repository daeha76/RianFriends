using Microsoft.EntityFrameworkCore;
using RianFriends.Application.Identity.Interfaces;
using RianFriends.Domain.Identity;
using RianFriends.Infrastructure.Persistence;

namespace RianFriends.Infrastructure.Identity;

/// <summary>User 저장소 구현체</summary>
internal sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    /// <summary>의존성을 주입합니다.</summary>
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null, ct);

    /// <inheritdoc />
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.DeletedAt == null, ct);

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await _context.Users.AnyAsync(u => u.Id == id, ct);

    /// <inheritdoc />
    public void Add(User user)
        => _context.Users.Add(user);

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
