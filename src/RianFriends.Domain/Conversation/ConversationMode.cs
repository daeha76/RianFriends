namespace RianFriends.Domain.Conversation;

/// <summary>대화 세션의 모드. 세션 단위로 유지되며 세션 종료 시 Language로 복귀합니다.</summary>
public enum ConversationMode
{
    /// <summary>기본 모드: 언어 학습 + 대화 (CodeSwitch, 문법 교정 활성)</summary>
    Language,

    /// <summary>공감 모드: 언어 교정 없음, 감정 공감 위주 (CodeSwitch, 문법 교정 비활성)</summary>
    Empathy
}
