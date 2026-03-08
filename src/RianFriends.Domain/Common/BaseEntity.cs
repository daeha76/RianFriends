namespace RianFriends.Domain.Common;

/// <summary>모든 도메인 엔티티의 기반 클래스. 도메인 이벤트 발행을 지원합니다.</summary>
public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>엔티티 고유 식별자</summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>발행 대기 중인 도메인 이벤트 목록 (읽기 전용)</summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>도메인 이벤트를 발행 큐에 추가합니다.</summary>
    protected void AddDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    /// <summary>발행된 도메인 이벤트를 모두 제거합니다. Infrastructure에서 이벤트 처리 후 호출합니다.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
