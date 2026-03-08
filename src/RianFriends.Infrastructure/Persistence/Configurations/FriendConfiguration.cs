using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FriendEntity = RianFriends.Domain.Friend.Friend;

namespace RianFriends.Infrastructure.Persistence.Configurations;

internal sealed class FriendConfiguration : IEntityTypeConfiguration<FriendEntity>
{
    public void Configure(EntityTypeBuilder<FriendEntity> builder)
    {
        builder.ToTable("friends");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.UserId).IsRequired();
        builder.Property(f => f.PersonaId).IsRequired();
        builder.Property(f => f.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(f => f.CreatedAt).IsRequired();
        builder.Property(f => f.UpdatedAt).IsRequired();

        // 한 사용자가 같은 페르소나를 여러 번 만들 수 있으므로 유니크 제약 없음
        builder.HasIndex(f => f.UserId);
        builder.HasIndex(f => new { f.UserId, f.IsActive });

        // DomainEvent는 DB에 저장하지 않음 (무시)
        builder.Ignore(f => f.DomainEvents);
    }
}
