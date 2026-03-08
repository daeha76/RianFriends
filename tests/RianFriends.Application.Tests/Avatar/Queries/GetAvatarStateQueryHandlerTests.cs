using FluentAssertions;
using Moq;
using RianFriends.Application.Abstractions;
using RianFriends.Application.Avatar.Queries.GetAvatarState;
using AvatarEntity = RianFriends.Domain.Avatar.Avatar;

namespace RianFriends.Application.Tests.Avatar.Queries;

[Trait("Category", "Unit")]
public class GetAvatarStateQueryHandlerTests
{
    private readonly Mock<IAvatarRepository> _avatarRepositoryMock = new();
    private readonly GetAvatarStateQueryHandler _sut;

    private static readonly Guid ValidFriendId = Guid.NewGuid();

    public GetAvatarStateQueryHandlerTests()
    {
        _sut = new GetAvatarStateQueryHandler(_avatarRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenAvatarExists_ShouldReturnState()
    {
        // Arrange
        var avatar = AvatarEntity.Create(ValidFriendId).Value;
        avatar.IncreaseHunger(50); // HungerStatus = Hungry

        _avatarRepositoryMock
            .Setup(r => r.GetByFriendIdAsync(ValidFriendId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(avatar);

        var query = new GetAvatarStateQuery(ValidFriendId);

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

        var query = new GetAvatarStateQuery(ValidFriendId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.HungerLevel.Should().Be(0);
        result.Value.HungerStatus.Should().Be("Satisfied");
    }

    [Fact]
    public async Task Handle_WithEmptyFriendId_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetAvatarStateQuery(Guid.Empty);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
