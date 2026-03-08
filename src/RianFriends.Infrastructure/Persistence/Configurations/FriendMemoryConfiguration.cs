using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RianFriends.Domain.Memory;

namespace RianFriends.Infrastructure.Persistence.Configurations;

internal sealed class FriendMemoryConfiguration : IEntityTypeConfiguration<FriendMemory>
{
    public void Configure(EntityTypeBuilder<FriendMemory> builder)
    {
        builder.ToTable("friend_memories");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.FriendId).IsRequired();

        builder.Property(m => m.Layer)
            .HasConversion<string>()
            .HasColumnType("text")
            .IsRequired();

        builder.Property(m => m.Summary)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(m => m.ExpiresAt).IsRequired();
        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.UpdatedAt).IsRequired();

        // 만료 조회 최적화
        builder.HasIndex(m => new { m.FriendId, m.Layer });
        builder.HasIndex(m => m.ExpiresAt);

        builder.Ignore(m => m.DomainEvents);
    }
}
