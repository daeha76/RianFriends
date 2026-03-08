using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RianFriends.Domain.Notification;

namespace RianFriends.Infrastructure.Persistence.Configurations;

internal sealed class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DeviceToken> builder)
    {
        builder.ToTable("device_tokens");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.UserId).IsRequired();
        builder.Property(d => d.Token).IsRequired().HasColumnType("text").HasMaxLength(500);
        builder.Property(d => d.Platform).IsRequired()
            .HasConversion<string>()
            .HasColumnType("text")
            .HasMaxLength(10);
        builder.Property(d => d.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(d => d.CreatedAt).IsRequired();
        builder.Property(d => d.UpdatedAt).IsRequired();

        builder.Ignore(d => d.DomainEvents);

        builder.HasIndex(d => d.UserId);
        builder.HasIndex(d => new { d.UserId, d.IsActive });
        // 같은 토큰 값이 중복 등록되지 않도록
        builder.HasIndex(d => d.Token).IsUnique();
    }
}
