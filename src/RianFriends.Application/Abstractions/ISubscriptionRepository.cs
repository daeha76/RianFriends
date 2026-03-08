using RianFriends.Domain.Billing;

namespace RianFriends.Application.Abstractions;

/// <summary>구독 이력 저장소 인터페이스</summary>
public interface ISubscriptionRepository
{
    /// <summary>사용자의 현재 활성 구독을 가져옵니다.</summary>
    Task<Subscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>새 구독을 추가합니다.</summary>
    void Add(Subscription subscription);

    /// <summary>변경 사항을 저장합니다.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
