namespace RianFriends.Domain.Avatar;

/// <summary>
/// 아바타의 배고픔 상태를 나타내는 열거형.
/// HungerLevel 0–39: Satisfied, 40–69: Hungry, 70–100: Starving.
/// </summary>
public enum HungerStatus
{
    /// <summary>배부름 (0–39)</summary>
    Satisfied = 0,

    /// <summary>배고픔 (40–69)</summary>
    Hungry = 1,

    /// <summary>굶주림 (70–100) — 푸시 알림 발송 임계치</summary>
    Starving = 2,
}
