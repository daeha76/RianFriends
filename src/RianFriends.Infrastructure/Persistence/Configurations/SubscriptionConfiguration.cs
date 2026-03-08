using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RianFriends.Domain.Billing;
using RianFriends.Domain.Identity;

namespace RianFriends.Infrastructure.Persistence.Configurations;

/// <summary>Subscription 엔티티 EF Core 매핑 설정</summary>
internal sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId).IsRequired();

        builder.Property(s => s.RevenueCatCustomerId)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(s => s.ProductId)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(s => s.PlanType)
            .HasConversion<string>()
            .HasColumnType("text")
            .IsRequired();

        builder.Property(s => s.StartsAt).IsRequired();
        builder.Property(s => s.ExpiresAt).IsRequired();
        builder.Property(s => s.IsActive).IsRequired().HasDefaultValue(true);

        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        // DomainEvents는 DB에 저장하지 않음
        builder.Ignore(s => s.DomainEvents);

        // 사용자 기준 조회 최적화
        builder.HasIndex(s => s.UserId);
        // 활성 구독 조회 최적화
        builder.HasIndex(s => new { s.UserId, s.IsActive });
    }
}
