using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Conversation;

namespace RianFriends.Infrastructure.Redis;

/// <summary>
/// Redis 기반 대화 컨텍스트 관리 서비스.
/// Key: conversation:{sessionId}:context, TTL: 30분.
/// </summary>
internal sealed class RedisContextService : IRedisContextService
{
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan ContextTtl = TimeSpan.FromMinutes(30);
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public RedisContextService(IDistributedCache cache) => _cache = cache;

    private static string GetKey(Guid sessionId) => $"conversation:{sessionId}:context";

    public async Task<List<Message>> GetContextAsync(Guid sessionId, CancellationToken ct = default)
    {
        var json = await _cache.GetStringAsync(GetKey(sessionId), ct);
        if (string.IsNullOrEmpty(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<MessageCacheDto>>(json, JsonOptions)
            ?.Select(dto => CreateMessageFromCache(dto, sessionId))
            .Where(m => m is not null)
            .Select(m => m!)
            .ToList() ?? [];
    }

    public async Task SetContextAsync(Guid sessionId, List<Message> messages, CancellationToken ct = default)
    {
        var dtos = messages.Select(m => new MessageCacheDto(m.Id, m.Role, m.Content)).ToList();
        var json = JsonSerializer.Serialize(dtos, JsonOptions);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ContextTtl
        };

        await _cache.SetStringAsync(GetKey(sessionId), json, options, ct);
    }

    public Task RemoveContextAsync(Guid sessionId, CancellationToken ct = default)
        => _cache.RemoveAsync(GetKey(sessionId), ct);

    private static Message? CreateMessageFromCache(MessageCacheDto dto, Guid sessionId)
    {
        // Redis에서 복원 시 도메인 이벤트 없이 생성 (캐시 전용)
        var result = Message.Create(sessionId, dto.Role, dto.Content);
        return result.IsSuccess ? result.Value : null;
    }

    private sealed record MessageCacheDto(Guid Id, string Role, string Content);
}
