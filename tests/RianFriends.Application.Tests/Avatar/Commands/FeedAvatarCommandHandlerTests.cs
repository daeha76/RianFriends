using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RianFriends.Application.Abstractions;
using RianFriends.Application.Avatar.Commands.FeedAvatar;
using RianFriends.Domain.Avatar;
using AvatarEntity = RianFriends.Domain.Avatar.Avatar;
using FriendEntity = RianFriends.Domain.Friend.Friend;

namespace RianFriends.Application.Tests.Avatar.Commands;

[Trait("Category", "Unit")]
public class FeedAvatarCommandHandlerTests
{
    private readonly Mock<IAvatarRepository> _avatarRepositoryMock = new();
    private readonly Mock<IFriendRepository> _friendRepositoryMock = new();
    private readonly FeedAvatarCommandHandler _sut;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidFriendId = Guid.NewGuid();

    public FeedAvatarCommandHandlerTests()
    {
        _sut = new FeedAvatarCommandHandler(
            _avatarRepositoryMock.Object,
            _friendRepositoryMock.Object,
            NullLogger<FeedAvatarCommandHandler>.Instance);

        // 기본: 소유권 검증 통과
        var friend = CreateFriendWithId(ValidUserId, ValidFriendId);
        _friendRepositoryMock
            .Setup(r => r.GetByIdAsync(ValidFriendId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(friend);
    }

    [Fact]
    public async Task Handle_WhenAvatarExists_ShouldFeedAndReturnNewLevel()
    {
        // Arrange
        var avatar = AvatarEntity.Create(ValidFriendId).Value;
        avatar.IncreaseHunger(50);

        _avatarRepositoryMock
            .Setup(r => r.GetByFriendIdAsync(ValidFriendId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(avatar);

        _avatarRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new FeedAvatarCommand(ValidUserId, ValidFriendId, "cookie");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(30); // 50 - 20 = 30
        _avatarRepositoryMock.Verify(r => r.AddSnack(It.IsAny<Snack>()), Times.Once);
        _avatarRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAvatarNotExists_ShouldCreateAndFeed()
    {
        // Arrange
        _avatarRepositoryMock
            .Setup(r => r.GetByFriendIdAsync(ValidFriendId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AvatarEntity?)null);

        _avatarRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new FeedAvatarCommand(ValidUserId, ValidFriendId, "cookie");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0); // 새 아바타(0)에서 Feed → 0 미만 방지로 0
        _avatarRepositoryMock.Verify(r => r.Add(It.IsAny<AvatarEntity>()), Times.Once);
        _avatarRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenFriendNotOwnedByUser_ShouldReturnFailure()
    {
        // Arrange: 다른 사용자의 친구에 접근 시도 (IDOR 방어)
        var otherUserId = Guid.NewGuid();
        var command = new FeedAvatarCommand(otherUserId, ValidFriendId, "cookie");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _avatarRepositoryMock.Verify(r => r.Add(It.IsAny<AvatarEntity>()), Times.Never);
        _avatarRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidSnackType_ShouldNotSaveSnack()
    {
        // Arrange: 빈 snackType이면 Snack.Create 실패 → Snack은 저장 안 되지만 Feed는 성공
        var avatar = AvatarEntity.Create(ValidFriendId).Value;
        avatar.IncreaseHunger(30);

        _avatarRepositoryMock
            .Setup(r => r.GetByFriendIdAsync(ValidFriendId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(avatar);

        _avatarRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new FeedAvatarCommand(ValidUserId, ValidFriendId, ""); // 빈 snackType

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Feed 자체는 성공
        _avatarRepositoryMock.Verify(r => r.AddSnack(It.IsAny<Snack>()), Times.Never); // Snack 저장 안 됨
    }

    private static FriendEntity CreateFriendWithId(Guid userId, Guid friendId)
    {
        var friend = FriendEntity.Create(userId, Guid.NewGuid(), 0, 10).Value;
        typeof(FriendEntity).GetProperty("Id")!.SetValue(friend, friendId);
        return friend;
    }
}
