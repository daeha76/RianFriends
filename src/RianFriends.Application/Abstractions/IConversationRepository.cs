using RianFriends.Domain.Conversation;

namespace RianFriends.Application.Abstractions;

/// <summary>대화 세션 및 메시지 Repository 인터페이스</summary>
public interface IConversationRepository
{
    /// <summary>세션 ID로 대화 세션을 조회합니다.</summary>
    Task<ConversationSession?> GetSessionByIdAsync(Guid sessionId, CancellationToken ct = default);

    /// <summary>친구 ID에 대한 총 세션 수를 조회합니다.</summary>
    Task<int> CountSessionsByFriendIdAsync(Guid friendId, CancellationToken ct = default);

    /// <summary>세션의 최근 메시지 목록을 조회합니다.</summary>
    Task<List<Message>> GetRecentMessagesAsync(Guid sessionId, int count = 20, CancellationToken ct = default);

    /// <summary>메시지 ID로 메시지를 조회합니다.</summary>
    Task<Message?> GetMessageByIdAsync(Guid messageId, CancellationToken ct = default);

    /// <summary>세션을 추가합니다.</summary>
    void AddSession(ConversationSession session);

    /// <summary>메시지를 추가합니다.</summary>
    void AddMessage(Message message);

    /// <summary>변경사항을 저장합니다.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
