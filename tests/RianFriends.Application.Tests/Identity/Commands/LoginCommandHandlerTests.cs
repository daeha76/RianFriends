using FluentAssertions;
using Microsoft.Extensions.Configuration;
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
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly IConfiguration _configuration;
    private readonly LoginCommandHandler _sut;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly SocialUserInfo UserInfo = new(UserId, "user@example.com");

    public LoginCommandHandlerTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:AccessTokenExpirationMinutes"] = "60",
                ["Jwt:RefreshTokenExpirationDays"] = "30"
            })
            .Build();

        _jwtTokenServiceMock
            .Setup(j => j.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("test-access-token");
        _jwtTokenServiceMock
            .Setup(j => j.GenerateRefreshToken())
            .Returns("test-refresh-token");
        _jwtTokenServiceMock
            .Setup(j => j.HashToken(It.IsAny<string>()))
            .Returns("hashed-token");

        _sut = new LoginCommandHandler(
            _authServiceMock.Object,
            _jwtTokenServiceMock.Object,
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _configuration,
            NullLogger<LoginCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_OAuthLogin_NewUser_ShouldCreateUserAndIssueJwt()
    {
        // Arrange
        _authServiceMock
            .Setup(s => s.SignInWithOAuthAsync("google", "id-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(UserInfo);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new LoginCommand("google", "id-token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("test-access-token");
        result.Value.RefreshToken.Should().Be("test-refresh-token");
        _userRepositoryMock.Verify(r => r.Add(It.IsAny<User>()), Times.Once);
        _refreshTokenRepositoryMock.Verify(r => r.Add(It.IsAny<RefreshToken>()), Times.Once);
        _refreshTokenRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OAuthLogin_ExistingUser_ShouldNotCreateUser()
    {
        // Arrange
        _authServiceMock
            .Setup(s => s.SignInWithOAuthAsync("google", "id-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(UserInfo);

        var existingUser = User.Create(UserId, "user@example.com").Value;
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var command = new LoginCommand("google", "id-token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userRepositoryMock.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
        _jwtTokenServiceMock.Verify(j => j.GenerateAccessToken(UserId, "user@example.com", "user"), Times.Once);
    }

    [Fact]
    public async Task Handle_EmailLogin_ShouldCallSignInAsync()
    {
        // Arrange
        _authServiceMock
            .Setup(s => s.SignInAsync("user@example.com", "password", It.IsAny<CancellationToken>()))
            .ReturnsAsync(UserInfo);

        var existingUser = User.Create(UserId, "user@example.com").Value;
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var command = new LoginCommand("email", "password", Email: "user@example.com");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _authServiceMock.Verify(s => s.SignInAsync("user@example.com", "password", It.IsAny<CancellationToken>()), Times.Once);
        _authServiceMock.Verify(s => s.SignInWithOAuthAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
