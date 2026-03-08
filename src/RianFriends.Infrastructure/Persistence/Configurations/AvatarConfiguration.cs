using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AvatarEntity = RianFriends.Domain.Avatar.Avatar;
using RianFriends.Domain.Avatar;

namespace RianFriends.Infrastructure.Persistence.Configurations;

internal sealed class AvatarConfiguration : IEntityTypeConfiguration<AvatarEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AvatarEntity> builder)
    {
        builder.ToTable("avatars");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FriendId).IsRequired();
        builder.Property(a => a.HungerLevel).IsRequired().HasDefaultValue(0);
        builder.Property(a => a.LastFedAt).IsRequired();
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();

        // HungerStatus는 계산 프로퍼티이므로 DB 컬럼 제외
        builder.Ignore(a => a.HungerStatus);
        // DomainEvents는 DB에 저장하지 않음
        builder.Ignore(a => a.DomainEvents);

        // 친구 1명당 아바타 1개 (유니크 인덱스)
        builder.HasIndex(a => a.FriendId).IsUnique();
    }
}
