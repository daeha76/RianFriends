using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RianFriends.Domain.Avatar;

namespace RianFriends.Infrastructure.Persistence.Configurations;

internal sealed class SnackConfiguration : IEntityTypeConfiguration<Snack>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Snack> builder)
    {
        builder.ToTable("snacks");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.AvatarId).IsRequired();
        builder.Property(s => s.SnackType).IsRequired().HasColumnType("text").HasMaxLength(50);
        builder.Property(s => s.FedAt).IsRequired();
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        builder.Ignore(s => s.DomainEvents);

        builder.HasIndex(s => s.AvatarId);
    }
}
