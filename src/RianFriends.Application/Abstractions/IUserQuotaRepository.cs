using RianFriends.Domain.Billing;

namespace RianFriends.Application.Abstractions;

/// <summary>일일 토큰 쿼터 저장소 인터페이스</summary>
public interface IUserQuotaRepository
{
    /// <summary>특정 사용자의 오늘 날짜 쿼터를 가져옵니다.</summary>
    Task<UserQuota?> GetTodayAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>새 쿼터를 추가합니다.</summary>
    void Add(UserQuota quota);

    /// <summary>변경 사항을 저장합니다.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>오늘 날짜의 모든 쿼터를 초기화합니다 (배치 잡용).</summary>
    Task ResetAllAsync(CancellationToken cancellationToken = default);
}
