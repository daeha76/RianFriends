using Microsoft.EntityFrameworkCore;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Notification;
using RianFriends.Infrastructure.Persistence;

namespace RianFriends.Infrastructure.Notification;

/// <summary>IAlarmRepository 구현체</summary>
internal sealed class AlarmRepository : IAlarmRepository
{
    private readonly AppDbContext _context;

    /// <inheritdoc />
    public AlarmRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public Task<List<WakeUpAlarm>> GetByUserIdAsync(Guid userId, CancellationToken ct)
    {
        return _context.WakeUpAlarms
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.AlarmTime)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public Task<WakeUpAlarm?> GetByIdAsync(Guid alarmId, Guid userId, CancellationToken ct)
    {
        return _context.WakeUpAlarms
            .FirstOrDefaultAsync(a => a.Id == alarmId && a.UserId == userId, ct);
    }

    /// <inheritdoc />
    public void Add(WakeUpAlarm alarm)
    {
        _context.WakeUpAlarms.Add(alarm);
    }

    /// <inheritdoc />
    public void Remove(WakeUpAlarm alarm)
    {
        _context.WakeUpAlarms.Remove(alarm);
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return _context.SaveChangesAsync(ct);
    }
}
