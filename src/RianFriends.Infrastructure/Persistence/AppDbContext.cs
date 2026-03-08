using MediatR;
using Microsoft.EntityFrameworkCore;
using AvatarEntity = RianFriends.Domain.Avatar.Avatar;
using RianFriends.Application.Identity.Interfaces;
using RianFriends.Domain.Avatar;
using RianFriends.Domain.Billing;
using RianFriends.Domain.Common;
using RianFriends.Domain.Conversation;
using RianFriends.Domain.Friend;
using RianFriends.Domain.Identity;
using RianFriends.Domain.Learning;
using RianFriends.Domain.Memory;
using RianFriends.Domain.Notification;

namespace RianFriends.Infrastructure.Persistence;

/// <summary>
/// RianFriends 애플리케이션 데이터베이스 컨텍스트.
/// Supabase(PostgreSQL) 연결, snake_case 네이밍, Audit 자동 처리를 담당합니다.
/// </summary>
public class AppDbContext : DbContext
{
    private readonly IPublisher _publisher;
    private readonly ICurrentUserService? _currentUserService;

    /// <summary>사용자 테이블</summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>AI 친구 페르소나 테이블</summary>
    public DbSet<FriendPersona> FriendPersonas => Set<FriendPersona>();

    /// <summary>AI 친구 테이블</summary>
    public DbSet<Domain.Friend.Friend> Friends => Set<Domain.Friend.Friend>();

    /// <summary>친구 메모리 테이블</summary>
    public DbSet<FriendMemory> FriendMemories => Set<FriendMemory>();

    /// <summary>대화 세션 테이블</summary>
    public DbSet<ConversationSession> ConversationSessions => Set<ConversationSession>();

    /// <summary>메시지 테이블</summary>
    public DbSet<Message> Messages => Set<Message>();

    /// <summary>언어 레벨 기록 테이블</summary>
    public DbSet<LanguageLevelRecord> LanguageLevelRecords => Set<LanguageLevelRecord>();

    /// <summary>아바타 테이블</summary>
    public DbSet<AvatarEntity> Avatars => Set<AvatarEntity>();

    /// <summary>간식 기록 테이블</summary>
    public DbSet<Snack> Snacks => Set<Snack>();

    /// <summary>기상 알람 테이블</summary>
    public DbSet<WakeUpAlarm> WakeUpAlarms => Set<WakeUpAlarm>();

    /// <summary>디바이스 토큰 테이블</summary>
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();

    /// <summary>일일 토큰 쿼터 테이블</summary>
    public DbSet<UserQuota> UserQuotas => Set<UserQuota>();

    /// <summary>구독 이력 테이블</summary>
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    /// <summary>RefreshToken 테이블</summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>생성자</summary>
    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IPublisher publisher,
        ICurrentUserService? currentUserService = null) : base(options)
    {
        _publisher = publisher;
        _currentUserService = currentUserService;
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyReference).Assembly);
    }

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditColumns();
        var result = await base.SaveChangesAsync(cancellationToken);
        await DispatchDomainEventsAsync(cancellationToken);
        return result;
    }

    private async Task DispatchDomainEventsAsync(CancellationToken ct)
    {
        var entities = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities.SelectMany(e => e.DomainEvents).ToList();
        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, ct);
        }
    }

    private void UpdateAuditColumns()
    {
        var currentUserId = GetCurrentUserId();
        var entries = ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                entry.Entity.CreatedBy = currentUserId;
                entry.Entity.UpdatedBy = currentUserId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                entry.Entity.UpdatedBy = currentUserId;
            }
        }
    }

    private Guid? GetCurrentUserId()
    {
        if (_currentUserService is null)
        {
            return null;
        }

        var userId = _currentUserService.UserId;
        return userId == Guid.Empty ? null : userId;
    }
}
