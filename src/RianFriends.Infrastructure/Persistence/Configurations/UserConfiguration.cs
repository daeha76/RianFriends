using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RianFriends.Domain.Identity;

namespace RianFriends.Infrastructure.Persistence.Configurations;

/// <summary>User 엔티티 EF Core 매핑 설정</summary>
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>User 테이블 매핑을 구성합니다.</summary>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        // Id는 Supabase Auth user.id와 동일 (UUID) — DB 자동 생성 안 함
        builder.Property(u => u.Id)
            .ValueGeneratedNever();

        builder.Property(u => u.Email)
            .HasColumnType("text")
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("deleted_at IS NULL"); // 활성 계정만 이메일 유니크

        builder.Property(u => u.Nickname)
            .HasColumnType("text")
            .HasMaxLength(50);

        builder.Property(u => u.CountryCode)
            .HasColumnType("text")
            .HasMaxLength(2);

        builder.Property(u => u.Plan)
            .HasConversion<string>()
            .HasColumnType("text")
            .IsRequired();

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasColumnType("text")
            .IsRequired();

        // AuditableEntity 공통 컬럼
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt).IsRequired();
    }
}
