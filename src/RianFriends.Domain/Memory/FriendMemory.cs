using RianFriends.Domain.Common;

namespace RianFriends.Domain.Memory;

/// <summary>
/// 계층적 AI 친구 메모리 엔티티.
/// LLM이 생성한 요약 텍스트를 레이어별로 저장합니다.
/// 실시간 대화 시: ShortTerm + MidTerm만 컨텍스트로 주입.
/// 상위 레이어(Quarter 이상) 요약은 배치 잡(MemorySummaryJob)에서만 생성.
/// </summary>
public sealed class FriendMemory : AuditableEntity
{
    /// <summary>이 메모리가 속한 AI 친구 ID</summary>
    public Guid FriendId { get; private set; }

    /// <summary>메모리 계층 (시간 범위)</summary>
    public MemoryLayer Layer { get; private set; }

    /// <summary>LLM이 생성한 요약 텍스트</summary>
    public string Summary { get; private set; } = string.Empty;

    /// <summary>이 메모리 항목의 만료 시각 (UTC). 만료 후 상위 레이어로 요약 이동.</summary>
    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>EF Core용 기본 생성자</summary>
    private FriendMemory() { }

    /// <summary>새 메모리 항목을 생성합니다.</summary>
    public static Result<FriendMemory> Create(Guid friendId, MemoryLayer layer, string summary)
    {
        if (friendId == Guid.Empty)
        {
            return Result.Failure<FriendMemory>("친구 ID는 필수입니다.");
        }

        if (string.IsNullOrWhiteSpace(summary))
        {
            return Result.Failure<FriendMemory>("메모리 요약 내용은 필수입니다.");
        }

        var expiresAt = DateTimeOffset.UtcNow.Add(GetTtlForLayer(layer));

        var memory = new FriendMemory
        {
            FriendId = friendId,
            Layer = layer,
            Summary = summary,
            ExpiresAt = expiresAt
        };

        return Result.Success(memory);
    }

    /// <summary>레이어별 TTL(보존 기간)을 반환합니다.</summary>
    public static TimeSpan GetTtlForLayer(MemoryLayer layer) => layer switch
    {
        MemoryLayer.ShortTerm => TimeSpan.FromDays(7),
        MemoryLayer.MidTerm   => TimeSpan.FromDays(30),
        MemoryLayer.Quarter   => TimeSpan.FromDays(90),
        MemoryLayer.HalfYear  => TimeSpan.FromDays(180),
        MemoryLayer.Annual    => TimeSpan.FromDays(365),
        MemoryLayer.Decade    => TimeSpan.FromDays(3650),
        _ => throw new ArgumentOutOfRangeException(nameof(layer))
    };

    /// <summary>이 메모리 항목이 만료되었는지 확인합니다.</summary>
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
}
