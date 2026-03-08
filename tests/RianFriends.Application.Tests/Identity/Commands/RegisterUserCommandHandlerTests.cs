using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RianFriends.Application.Identity.Commands.RegisterUser;
using RianFriends.Application.Identity.Interfaces;
using RianFriends.Domain.Identity;

namespace RianFriends.Application.Tests.Identity.Commands;

[Trait("Category", "Unit")]
public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly RegisterUserCommandHandler _sut;

    private static readonly Guid UserId = Guid.NewGuid();

    public RegisterUserCommandHandlerTests()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(UserId);

        _sut = new RegisterUserCommandHandler(
            _userRepositoryMock.Object,
            _currentUserMock.Object,
            NullLogger<RegisterUserCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateProfileAndReturnDto()
    {
        // Arrange
        var user = User.Create(UserId, "test@example.com").Value;
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var birthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-20));
        var command = new RegisterUserCommand("리안", birthDate, "KR");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Nickname.Should().Be("리안");
        result.Value.CountryCode.Should().Be("KR");
        _userRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnFailure()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var birthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-20));
        var command = new RegisterUserCommand("리안", birthDate, "KR");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _userRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UnderageUser_ShouldReturnFailure()
    {
        // Arrange
        var user = User.Create(UserId, "test@example.com").Value;
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var underageDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-12));
        var command = new RegisterUserCommand("리안", underageDate, "KR");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("13세");
    }
}
