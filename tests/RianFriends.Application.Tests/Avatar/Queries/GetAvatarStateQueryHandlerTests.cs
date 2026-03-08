using FluentAssertions;
using Moq;
using RianFriends.Application.Abstractions;
using RianFriends.Application.Avatar.Queries.GetAvatarState;
using AvatarEntity = RianFriends.Domain.Avatar.Avatar;
using FriendEntity = RianFriends.Domain.Friend.Friend;

namespace RianFriends.Application.Tests.Avatar.Queries;

[Trait("Category", "Unit")]
public class GetAvatarStateQueryHandlerTests
{
    private readonly Mock<IAvatarRepository> _avatarRepositoryMock = new();
    private readonly Mock<IFriendRepository> _friendRepositoryMock = new();
    private readonly GetAvatarStateQueryHandler _sut;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidFriendId = Guid.NewGuid();

    public GetAvatarStateQueryHandlerTests()
    {
        _sut = new GetAvatarStateQueryHandler(
            _avatarRepositoryMock.Object,
            _friendRepositoryMock.Object);

        // 기본: 소유권 검증 통과
        var friend = CreateFriendWithId(ValidUserId, ValidFriendId);
        _friendRepositoryMock
            .Setup(r => r.GetByIdAsync(ValidFriendId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(friend);
    }

    [Fact]
    public async Task Handle_WhenAvatarExists_ShouldReturnState()
    {
        // Arrange
        var avatar = AvatarEntity.Create(ValidFriendId).Value;
        avatar.IncreaseHunger(50);

        _avatarRepositoryMock
            .Setup(r => r.GetByFriendIdAsync(ValidFriendId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(avatar);

        var query = new GetAvatarStateQuery(ValidUserId, ValidFriendId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FriendId.Should().Be(ValidFriendId);
        result.Value.HungerLevel.Should().Be(50);
        result.Value.HungerStatus.Should().Be("Hungry");
    }

    [Fact]
    public async Task Handle_WhenAvatarNotExists_ShouldReturnDefaultSatisfiedState()
    {
        // Arrange
        _avatarRepositoryMock
            .Setup(r => r.GetByFriendIdAsync(ValidFriendId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AvatarEntity?)null);

        var query = new GetAvatarStateQuery(ValidUserId, ValidFriendId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.HungerLevel.Should().Be(0);
        result.Value.HungerStatus.Should().Be("Satisfied");
    }

    [Fact]
    public async Task Handle_WhenFriendNotOwnedByUser_ShouldReturnFailure()
    {
        // Arrange: 다른 사용자의 친구에 접근 시도 (IDOR 방어)
        var otherUserId = Guid.NewGuid();
        var query = new GetAvatarStateQuery(otherUserId, ValidFriendId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    private static FriendEntity CreateFriendWithId(Guid userId, Guid friendId)
    {
        var friend = FriendEntity.Create(userId, Guid.NewGuid(), 0, 10).Value;
        typeof(FriendEntity).GetProperty("Id")!.SetValue(friend, friendId);
        return friend;
    }
}
