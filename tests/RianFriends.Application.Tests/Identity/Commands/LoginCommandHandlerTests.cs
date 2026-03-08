using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RianFriends.Application.Identity.Commands.Login;
using RianFriends.Application.Identity.Dtos;
using RianFriends.Application.Identity.Interfaces;
using RianFriends.Domain.Identity;

namespace RianFriends.Application.Tests.Identity.Commands;

[Trait("Category", "Unit")]
public class LoginCommandHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly LoginCommandHandler _sut;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly AuthResultDto AuthResult = new(
        UserId, "user@example.com", "access-token", "refresh-token", DateTimeOffset.UtcNow.AddHours(1));

    public LoginCommandHandlerTests()
    {
        _sut = new LoginCommandHandler(
            _authServiceMock.Object,
            _userRepositoryMock.Object,
            NullLogger<LoginCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_OAuthLogin_NewUser_ShouldCreateUserInDb()
    {
        // Arrange
        _authServiceMock
            .Setup(s => s.SignInWithOAuthAsync("google", "id-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(AuthResult);

        _userRepositoryMock
            .Setup(r => r.ExistsAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new LoginCommand("google", "id-token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userRepositoryMock.Verify(r => r.Add(It.IsAny<User>()), Times.Once);
        _userRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OAuthLogin_ExistingUser_ShouldNotCreateUserInDb()
    {
        // Arrange
        _authServiceMock
            .Setup(s => s.SignInWithOAuthAsync("google", "id-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(AuthResult);

        _userRepositoryMock
            .Setup(r => r.ExistsAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new LoginCommand("google", "id-token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userRepositoryMock.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmailLogin_ShouldCallSignInAsync()
    {
        // Arrange
        _authServiceMock
            .Setup(s => s.SignInAsync("user@example.com", "password", It.IsAny<CancellationToken>()))
            .ReturnsAsync(AuthResult);

        _userRepositoryMock
            .Setup(r => r.ExistsAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new LoginCommand("email", "password", Email: "user@example.com");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _authServiceMock.Verify(s => s.SignInAsync("user@example.com", "password", It.IsAny<CancellationToken>()), Times.Once);
        _authServiceMock.Verify(s => s.SignInWithOAuthAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
