using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RianFriends.Domain.Learning;

namespace RianFriends.Infrastructure.Persistence.Configurations;

internal sealed class LanguageLevelRecordConfiguration : IEntityTypeConfiguration<LanguageLevelRecord>
{
    public void Configure(EntityTypeBuilder<LanguageLevelRecord> builder)
    {
        builder.ToTable("language_level_records");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.UserId).IsRequired();
        builder.Property(r => r.FriendId).IsRequired();

        builder.Property(r => r.Language)
            .HasColumnType("text")
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(r => r.Level)
            .HasConversion<string>()
            .HasColumnType("text")
            .IsRequired();

        builder.Property(r => r.EvaluatedAt).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();

        // 언어별 독립 레벨 지원
        builder.HasIndex(r => new { r.UserId, r.FriendId, r.Language }).IsUnique();

        builder.Ignore(r => r.DomainEvents);
    }
}
