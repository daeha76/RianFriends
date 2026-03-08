using RianFriends.Domain.Common;
using RianFriends.Domain.Friend.Events;

namespace RianFriends.Domain.Friend;

/// <summary>
/// AI 친구 엔티티. 사용자(UserId)와 페르소나(FriendPersona)를 연결합니다.
/// 무료 플랜: 1명, Basic: 3명, Plus 이상: 5명+.
/// </summary>
public sealed class Friend : AuditableEntity
{
    /// <summary>이 친구를 소유한 사용자 ID</summary>
    public Guid UserId { get; private set; }

    /// <summary>친구의 페르소나 ID</summary>
    public Guid PersonaId { get; private set; }

    /// <summary>활성 여부 (비활성화해도 데이터는 유지)</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>EF Core용 기본 생성자</summary>
    private Friend() { }

    /// <summary>새 AI 친구를 생성합니다.</summary>
    /// <param name="userId">소유 사용자 ID</param>
    /// <param name="personaId">선택한 페르소나 ID</param>
    /// <param name="currentFriendCount">현재 사용자의 친구 수 (플랜 제한 검사용)</param>
    /// <param name="maxFriendCount">플랜별 최대 친구 수</param>
    public static Result<Friend> Create(Guid userId, Guid personaId, int currentFriendCount, int maxFriendCount)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<Friend>("사용자 ID는 필수입니다.");
        }

        if (personaId == Guid.Empty)
        {
            return Result.Failure<Friend>("페르소나 ID는 필수입니다.");
        }

        if (currentFriendCount >= maxFriendCount)
        {
            return Result.Failure<Friend>($"현재 플랜에서는 최대 {maxFriendCount}명의 친구만 만들 수 있습니다.");
        }

        var friend = new Friend
        {
            UserId = userId,
            PersonaId = personaId,
            IsActive = true
        };

        friend.AddDomainEvent(new FriendCreatedEvent(friend.Id, userId, personaId));
        return Result.Success(friend);
    }

    /// <summary>친구를 비활성화합니다 (삭제 대신 비활성화).</summary>
    public Result Deactivate()
    {
        if (!IsActive)
        {
            return Result.Failure("이미 비활성화된 친구입니다.");
        }

        IsActive = false;
        return Result.Success();
    }
}
