using FriendEntity = RianFriends.Domain.Friend.Friend;
using FriendPersona = RianFriends.Domain.Friend.FriendPersona;

namespace RianFriends.Application.Abstractions;

/// <summary>AI 친구 Repository 인터페이스</summary>
public interface IFriendRepository
{
    /// <summary>ID로 친구를 조회합니다.</summary>
    Task<FriendEntity?> GetByIdAsync(Guid friendId, CancellationToken ct = default);

    /// <summary>사용자 ID로 친구 목록을 조회합니다.</summary>
    Task<List<FriendEntity>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>사용자의 활성 친구 수를 조회합니다.</summary>
    Task<int> CountActiveByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>페르소나 ID로 페르소나를 조회합니다.</summary>
    Task<FriendPersona?> GetPersonaByIdAsync(Guid personaId, CancellationToken ct = default);

    /// <summary>모든 페르소나 목록을 조회합니다.</summary>
    Task<List<FriendPersona>> GetAllPersonasAsync(CancellationToken ct = default);

    /// <summary>친구를 추가합니다.</summary>
    void Add(FriendEntity friend);

    /// <summary>변경사항을 저장합니다.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
