namespace RianFriends.Domain.Common;

/// <summary>
/// Audit 컬럼(생성/수정 일시, 사용자)을 가진 엔티티 기반 클래스.
/// 모든 비즈니스 엔티티는 이 클래스를 상속합니다.
/// setter는 internal — Infrastructure(AppDbContext)에서만 설정 가능.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>레코드 생성 일시 (UTC)</summary>
    public DateTimeOffset CreatedAt { get; internal set; } = DateTimeOffset.UtcNow;

    /// <summary>레코드 마지막 수정 일시 (UTC)</summary>
    public DateTimeOffset UpdatedAt { get; internal set; } = DateTimeOffset.UtcNow;

    /// <summary>레코드 생성한 사용자 ID (nullable: 시스템 생성 가능)</summary>
    public Guid? CreatedBy { get; internal set; }

    /// <summary>레코드 마지막 수정한 사용자 ID</summary>
    public Guid? UpdatedBy { get; internal set; }
}
