using RianFriends.Domain.Identity;

namespace RianFriends.Application.Identity.Interfaces;

/// <summary>사용자 저장소 인터페이스</summary>
public interface IUserRepository
{
    /// <summary>ID로 사용자를 조회합니다.</summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>이메일로 사용자를 조회합니다.</summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>사용자 존재 여부를 확인합니다.</summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);

    /// <summary>사용자를 추가합니다.</summary>
    void Add(User user);

    /// <summary>변경사항을 저장합니다.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
