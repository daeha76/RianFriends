using RianFriends.Domain.Conversation;

namespace RianFriends.Application.Abstractions;

/// <summary>
/// Redis 세션 컨텍스트 서비스 인터페이스.
/// Key: conversation:{sessionId}:context, TTL: 30분.
/// 세션 내 최근 메시지를 빠르게 조회하기 위해 사용합니다.
/// </summary>
public interface IRedisContextService
{
    /// <summary>세션 컨텍스트 메시지 목록을 조회합니다.</summary>
    Task<List<Message>> GetContextAsync(Guid sessionId, CancellationToken ct = default);

    /// <summary>세션 컨텍스트 메시지 목록을 저장합니다 (TTL: 30분).</summary>
    Task SetContextAsync(Guid sessionId, List<Message> messages, CancellationToken ct = default);

    /// <summary>세션 종료 시 컨텍스트를 삭제합니다.</summary>
    Task RemoveContextAsync(Guid sessionId, CancellationToken ct = default);
}
