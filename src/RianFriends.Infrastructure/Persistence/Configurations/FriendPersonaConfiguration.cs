using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RianFriends.Domain.Friend;

namespace RianFriends.Infrastructure.Persistence.Configurations;

internal sealed class FriendPersonaConfiguration : IEntityTypeConfiguration<FriendPersona>
{
    public void Configure(EntityTypeBuilder<FriendPersona> builder)
    {
        builder.ToTable("friend_personas");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasColumnType("text")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Nationality)
            .HasColumnType("text")
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(p => p.TargetLanguage)
            .HasColumnType("text")
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(p => p.Personality)
            .HasConversion<string>()
            .HasColumnType("text")
            .IsRequired();

        builder.Property(p => p.Interests)
            .HasColumnType("text[]")
            .IsRequired();

        builder.Property(p => p.SpeechStyle)
            .HasConversion<string>()
            .HasColumnType("text")
            .IsRequired();

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();
    }
}
