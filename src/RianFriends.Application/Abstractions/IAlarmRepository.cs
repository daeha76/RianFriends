using RianFriends.Domain.Notification;

namespace RianFriends.Application.Abstractions;

/// <summary>기상 알람 영속성 추상화</summary>
public interface IAlarmRepository
{
    /// <summary>사용자 ID로 알람 목록을 조회합니다.</summary>
    Task<List<WakeUpAlarm>> GetByUserIdAsync(Guid userId, CancellationToken ct);

    /// <summary>알람 ID와 사용자 ID로 단일 알람을 조회합니다.</summary>
    Task<WakeUpAlarm?> GetByIdAsync(Guid alarmId, Guid userId, CancellationToken ct);

    /// <summary>알람을 추가합니다.</summary>
    void Add(WakeUpAlarm alarm);

    /// <summary>알람을 제거합니다.</summary>
    void Remove(WakeUpAlarm alarm);

    /// <summary>변경사항을 저장합니다.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct);
}
