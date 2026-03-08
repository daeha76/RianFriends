namespace RianFriends.Domain.Friend;

/// <summary>AI 친구의 말투 스타일</summary>
public enum SpeechStyle
{
    /// <summary>격식체 (정중한 존댓말)</summary>
    Formal,

    /// <summary>일상 구어체 (편안한 반말)</summary>
    Casual,

    /// <summary>이모지를 많이 사용하는 스타일</summary>
    EmojiHeavy,

    /// <summary>격식체와 구어체 혼용</summary>
    Mixed
}
