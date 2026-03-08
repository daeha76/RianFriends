using RianFriends.Domain.Common;
using RianFriends.Domain.Identity;

namespace RianFriends.Domain.Billing;

/// <summary>
/// 일일 토큰 쿼터 엔티티.
/// 플랜별 한도(Free:3K / Basic:20K / Plus:100K / Pro:무제한)를 추적합니다.
/// </summary>
public sealed class UserQuota : AuditableEntity
{
    /// <summary>사용자 ID</summary>
    public Guid UserId { get; private set; }

    /// <summary>쿼터 기준일 (UTC)</summary>
    public DateOnly Date { get; private set; }

    /// <summary>오늘 사용한 토큰 수</summary>
    public int UsedTokens { get; private set; }

    /// <summary>일일 토큰 한도</summary>
    public int QuotaLimit { get; private set; }

    /// <summary>EF Core용 기본 생성자</summary>
    private UserQuota() { }

    /// <summary>플랜 유형별 일일 토큰 한도</summary>
    private static readonly Dictionary<PlanType, int> PlanLimits = new()
    {
        { PlanType.Free,  3_000 },
        { PlanType.Basic, 20_000 },
        { PlanType.Plus,  100_000 },
        { PlanType.Pro,   int.MaxValue }
    };

    /// <summary>오늘 날짜 기준으로 쿼터를 생성합니다.</summary>
    /// <param name="userId">사용자 ID</param>
    /// <param name="planType">현재 구독 플랜</param>
    public static Result<UserQuota> Create(Guid userId, PlanType planType)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<UserQuota>("사용자 ID가 유효하지 않습니다.");
        }

        var quota = new UserQuota
        {
            UserId = userId,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            UsedTokens = 0,
            QuotaLimit = PlanLimits[planType]
        };

        return Result.Success(quota);
    }

    /// <summary>
    /// 토큰을 소비합니다.
    /// 한도를 초과하면 실패를 반환합니다.
    /// </summary>
    /// <param name="tokens">소비할 토큰 수</param>
    public Result Consume(int tokens)
    {
        if (tokens <= 0)
        {
            return Result.Failure("소비 토큰 수는 1 이상이어야 합니다.");
        }

        if (UsedTokens + tokens > QuotaLimit)
        {
            return Result.Failure($"일일 토큰 한도({QuotaLimit:N0})를 초과합니다. 현재 사용: {UsedTokens:N0}");
        }

        UsedTokens += tokens;
        return Result.Success();
    }

    /// <summary>잔여 토큰이 있는지 확인합니다.</summary>
    public bool HasRemainingTokens() => UsedTokens < QuotaLimit;

    /// <summary>일일 쿼터를 초기화합니다 (배치 잡에서 호출).</summary>
    public void Reset()
    {
        UsedTokens = 0;
        Date = DateOnly.FromDateTime(DateTime.UtcNow);
    }

    /// <summary>플랜 변경 시 한도를 업데이트합니다.</summary>
    /// <param name="newPlan">새 구독 플랜</param>
    public void UpdateLimit(PlanType newPlan)
    {
        QuotaLimit = PlanLimits[newPlan];
    }
}
