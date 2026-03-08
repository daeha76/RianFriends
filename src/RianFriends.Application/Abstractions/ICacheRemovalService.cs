namespace RianFriends.Application.Abstractions;

/// <summary>
/// 캐시 키 삭제 인터페이스.
/// Infrastructure 레이어의 Redis 구현체에서 실제 삭제를 처리합니다.
/// </summary>
public interface ICacheRemovalService
{
    /// <summary>지정된 캐시 키를 삭제합니다.</summary>
    Task RemoveAsync(string key, CancellationToken ct = default);
}
