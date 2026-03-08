using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RianFriends.Domain.Conversation;

namespace RianFriends.Infrastructure.Persistence.Configurations;

internal sealed class ConversationSessionConfiguration : IEntityTypeConfiguration<ConversationSession>
{
    public void Configure(EntityTypeBuilder<ConversationSession> builder)
    {
        builder.ToTable("conversation_sessions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FriendId).IsRequired();
        builder.Property(s => s.UserId).IsRequired();
        builder.Property(s => s.SessionNumber).IsRequired();

        // EmpathySettings를 Owned Entity (Owned Type)로 매핑
        builder.OwnsOne(s => s.EmpathySettings, empathy =>
        {
            empathy.Property(e => e.Gauge)
                .HasColumnName("empathy_gauge")
                .HasDefaultValue(0);

            empathy.Property(e => e.Mode)
                .HasColumnName("conversation_mode")
                .HasConversion<string>()
                .HasColumnType("text")
                .HasDefaultValue(ConversationMode.Language);

            empathy.Property(e => e.ControlMode)
                .HasColumnName("gauge_control_mode")
                .HasConversion<string>()
                .HasColumnType("text")
                .HasDefaultValue(GaugeControlMode.Auto);
        });

        builder.Property(s => s.EndedAt);
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        builder.HasIndex(s => new { s.FriendId, s.CreatedAt });
        builder.HasIndex(s => new { s.UserId, s.FriendId });

        builder.Ignore(s => s.DomainEvents);
        builder.Ignore(s => s.Mode); // EmpathySettings.Mode에서 파생
    }
}
