namespace RianFriends.Domain.Conversation;

/// <summary>공감 게이지 제어 방식</summary>
public enum GaugeControlMode
{
    /// <summary>자동 감지 활성 (기본값). 부정 감정 키워드 감지 시 공감 모드 전환 제안.</summary>
    Auto,

    /// <summary>사용자 수동 설정 중. ManualOverride 상태에서는 자동 제안을 표시하지 않음.</summary>
    ManualOverride
}
