using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RianFriends.Domain.Notification;

namespace RianFriends.Infrastructure.Persistence.Configurations;

internal sealed class WakeUpAlarmConfiguration : IEntityTypeConfiguration<WakeUpAlarm>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<WakeUpAlarm> builder)
    {
        builder.ToTable("wake_up_alarms");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.FriendId).IsRequired();
        builder.Property(a => a.AlarmTime).IsRequired();
        builder.Property(a => a.IsEnabled).IsRequired().HasDefaultValue(true);
        builder.Property(a => a.RepeatDays).IsRequired().HasDefaultValue((byte)0);
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();

        builder.Ignore(a => a.DomainEvents);

        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => new { a.UserId, a.IsEnabled });
    }
}
