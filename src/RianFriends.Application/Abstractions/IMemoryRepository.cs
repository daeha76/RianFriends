using RianFriends.Domain.Memory;

namespace RianFriends.Application.Abstractions;

/// <summary>계층형 친구 메모리 Repository 인터페이스</summary>
public interface IMemoryRepository
{
    /// <summary>대화 컨텍스트용 ShortTerm + MidTerm 메모리를 조회합니다.</summary>
    Task<List<FriendMemory>> GetContextMemoriesAsync(Guid friendId, CancellationToken ct = default);

    /// <summary>배치 처리 대상(만료된) 메모리를 조회합니다.</summary>
    Task<List<FriendMemory>> GetExpiredMemoriesAsync(MemoryLayer layer, CancellationToken ct = default);

    /// <summary>메모리를 추가합니다.</summary>
    void Add(FriendMemory memory);

    /// <summary>변경사항을 저장합니다.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
