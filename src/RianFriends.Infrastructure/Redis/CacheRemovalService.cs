using Microsoft.Extensions.Caching.Distributed;
using RianFriends.Application.Abstractions;

namespace RianFriends.Infrastructure.Redis;

/// <summary>IDistributedCache를 래핑한 캐시 삭제 서비스 구현체.</summary>
internal sealed class CacheRemovalService : ICacheRemovalService
{
    private readonly IDistributedCache _cache;

    public CacheRemovalService(IDistributedCache cache) => _cache = cache;

    /// <inheritdoc />
    public Task RemoveAsync(string key, CancellationToken ct = default)
        => _cache.RemoveAsync(key, ct);
}
