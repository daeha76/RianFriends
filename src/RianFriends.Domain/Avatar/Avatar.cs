using RianFriends.Domain.Avatar.Events;
using RianFriends.Domain.Common;

namespace RianFriends.Domain.Avatar;

/// <summary>
/// AI 친구 아바타 엔티티.
/// 배고픔 레벨(0–100)을 추적하며, 먹이기/배고픔 증가 메서드를 제공합니다.
/// 배고픔 레벨이 70 이상이 되면 AvatarHungryEvent를 발행합니다.
/// </summary>
public sealed class Avatar : AuditableEntity
{
    /// <summary>연결된 AI 친구 ID (1:1 관계)</summary>
    public Guid FriendId { get; private set; }

    /// <summary>배고픔 레벨 (0=배부름, 100=굶주림)</summary>
    public int HungerLevel { get; private set; }

    /// <summary>마지막으로 먹인 시각</summary>
    public DateTimeOffset LastFedAt { get; private set; }

    /// <summary>현재 배고픔 상태 (계산 프로퍼티)</summary>
    public HungerStatus HungerStatus => HungerLevel switch
    {
        < 40 => HungerStatus.Satisfied,
        < 70 => HungerStatus.Hungry,
        _ => HungerStatus.Starving,
    };

    /// <summary>EF Core용 기본 생성자</summary>
    private Avatar() { }

    /// <summary>새 아바타를 생성합니다.</summary>
    /// <param name="friendId">연결할 AI 친구 ID</param>
    public static Result<Avatar> Create(Guid friendId)
    {
        if (friendId == Guid.Empty)
        {
            return Result.Failure<Avatar>("친구 ID는 필수입니다.");
        }

        var avatar = new Avatar
        {
            FriendId = friendId,
            HungerLevel = 0,
            LastFedAt = DateTimeOffset.UtcNow,
        };

        return Result.Success(avatar);
    }

    /// <summary>
    /// 먹이를 줍니다. 배고픔 레벨을 감소시키고 AvatarFedEvent를 발행합니다.
    /// </summary>
    /// <param name="amount">감소량 (양수, 기본값 20)</param>
    public Result Feed(int amount = 20)
    {
        if (amount <= 0)
        {
            return Result.Failure("먹이 양은 1 이상이어야 합니다.");
        }

        HungerLevel = Math.Max(0, HungerLevel - amount);
        LastFedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new AvatarFedEvent(Id, HungerLevel));

        return Result.Success();
    }

    /// <summary>
    /// 배고픔 레벨을 증가시킵니다. 70 이상이면 AvatarHungryEvent를 발행합니다.
    /// HungerIncreaseJob에서 주기적으로 호출됩니다.
    /// </summary>
    /// <param name="amount">증가량 (기본값 5)</param>
    public Result IncreaseHunger(int amount = 5)
    {
        if (amount <= 0)
        {
            return Result.Failure("배고픔 증가량은 1 이상이어야 합니다.");
        }

        var wasBelow70 = HungerLevel < 70;
        HungerLevel = Math.Min(100, HungerLevel + amount);

        if (wasBelow70 && HungerLevel >= 70)
        {
            AddDomainEvent(new AvatarHungryEvent(Id, FriendId, HungerLevel));
        }

        return Result.Success();
    }
}
