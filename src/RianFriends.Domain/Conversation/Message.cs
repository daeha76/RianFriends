using RianFriends.Domain.Common;
using RianFriends.Domain.Conversation.Events;
using RianFriends.Domain.Learning;

namespace RianFriends.Domain.Conversation;

/// <summary>
/// 대화 내 개별 메시지 엔티티.
/// CodeSwitchData(JSONB)에는 비동기 후처리로 파싱된 CodeSwitchSegment 결과가 저장됩니다.
/// </summary>
public sealed class Message : AuditableEntity
{
    /// <summary>이 메시지가 속한 대화 세션 ID</summary>
    public Guid SessionId { get; private set; }

    /// <summary>발화 주체 ("user" 또는 "assistant")</summary>
    public string Role { get; private set; } = string.Empty;

    /// <summary>메시지 본문</summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// Code-Switching 파싱 결과 (JSONB, nullable).
    /// 메시지 저장 후 비동기 후처리로 채워집니다.
    /// 공감 모드(ConversationMode.Empathy)에서는 항상 null입니다.
    /// </summary>
    public CodeSwitchSegment[]? CodeSwitchData { get; private set; }

    /// <summary>EF Core용 기본 생성자</summary>
    private Message() { }

    /// <summary>새 메시지를 생성합니다.</summary>
    public static Result<Message> Create(Guid sessionId, string role, string content)
    {
        if (sessionId == Guid.Empty)
        {
            return Result.Failure<Message>("세션 ID는 필수입니다.");
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            return Result.Failure<Message>("발화 주체(role)는 필수입니다.");
        }

        if (role is not ("user" or "assistant"))
        {
            return Result.Failure<Message>("role은 'user' 또는 'assistant'여야 합니다.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return Result.Failure<Message>("메시지 내용은 필수입니다.");
        }

        var message = new Message
        {
            SessionId = sessionId,
            Role = role,
            Content = content
        };

        message.AddDomainEvent(new MessageSentEvent(message.Id, sessionId, role));
        return Result.Success(message);
    }

    /// <summary>
    /// Code-Switching 파싱 결과를 저장합니다. (비동기 후처리에서 호출)
    /// </summary>
    public void SetCodeSwitchData(CodeSwitchSegment[] segments)
    {
        CodeSwitchData = segments;
    }
}
