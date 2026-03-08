using AvatarEntity = RianFriends.Domain.Avatar.Avatar;

namespace RianFriends.Application.Abstractions;

/// <summary>아바타 영속성 추상화</summary>
public interface IAvatarRepository
{
    /// <summary>친구 ID로 아바타를 조회합니다.</summary>
    Task<AvatarEntity?> GetByFriendIdAsync(Guid friendId, CancellationToken ct);

    /// <summary>아바타를 추가합니다.</summary>
    void Add(AvatarEntity avatar);

    /// <summary>변경사항을 저장합니다.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct);
}
