using Microsoft.EntityFrameworkCore;
using AvatarEntity = RianFriends.Domain.Avatar.Avatar;
using RianFriends.Domain.Avatar;
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

    /// <summary>생성자</summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Infrastructure 어셈블리의 모든 IEntityTypeConfiguration 자동 등록
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyReference).Assembly);
    }

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditColumns();
        var result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }

    /// <summary>
    /// SaveChanges 전 Audit 컬럼(UpdatedAt)을 자동으로 갱신합니다.
    /// DB Trigger와 병행 사용하지만, 애플리케이션 레벨에서도 일관성을 보장합니다.
    /// </summary>
    private void UpdateAuditColumns()
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }
    }
}
