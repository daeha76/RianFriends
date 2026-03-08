using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RianFriends.Domain.Conversation;
using RianFriends.Domain.Learning;

namespace RianFriends.Infrastructure.Persistence.Configurations;

internal sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.SessionId).IsRequired();

        builder.Property(m => m.Role)
            .HasColumnType("text")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(m => m.Content)
            .HasColumnType("text")
            .IsRequired();

        // CodeSwitchData는 JSONB 컬럼 (nullable)
        builder.Property(m => m.CodeSwitchData)
            .HasColumnType("jsonb")
            .HasColumnName("code_switch_data");

        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.UpdatedAt).IsRequired();

        builder.HasIndex(m => m.SessionId);

        builder.Ignore(m => m.DomainEvents);
    }
}
