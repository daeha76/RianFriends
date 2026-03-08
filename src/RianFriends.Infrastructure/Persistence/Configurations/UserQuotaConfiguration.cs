using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RianFriends.Domain.Billing;

namespace RianFriends.Infrastructure.Persistence.Configurations;

/// <summary>UserQuota 엔티티 EF Core 매핑 설정</summary>
internal sealed class UserQuotaConfiguration : IEntityTypeConfiguration<UserQuota>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UserQuota> builder)
    {
        builder.ToTable("user_quotas");
        builder.HasKey(q => q.Id);

        builder.Property(q => q.UserId).IsRequired();
        builder.Property(q => q.Date).IsRequired();
        builder.Property(q => q.UsedTokens).IsRequired().HasDefaultValue(0);
        builder.Property(q => q.QuotaLimit).IsRequired();

        builder.Property(q => q.CreatedAt).IsRequired();
        builder.Property(q => q.UpdatedAt).IsRequired();

        // DomainEvents는 DB에 저장하지 않음
        builder.Ignore(q => q.DomainEvents);

        // 사용자별 날짜 유니크 (중복 방지)
        builder.HasIndex(q => new { q.UserId, q.Date }).IsUnique();
    }
}
