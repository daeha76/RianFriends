using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RianFriends.Application.Abstractions;
using RianFriends.Application.Billing.Queries.CheckTokenQuota;
using RianFriends.Application.Identity.Interfaces;
using RianFriends.Domain.Billing;
using RianFriends.Domain.Identity;

namespace RianFriends.Application.Tests.Billing;

[Trait("Category", "Unit")]
public class CheckTokenQuotaQueryHandlerTests
{
    private readonly Mock<IUserQuotaRepository> _quotaRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly CheckTokenQuotaQueryHandler _sut;

    private static readonly Guid ValidUserId = Guid.NewGuid();

    public CheckTokenQuotaQueryHandlerTests()
    {
        _sut = new CheckTokenQuotaQueryHandler(
            _quotaRepositoryMock.Object,
            _userRepositoryMock.Object,
            NullLogger<CheckTokenQuotaQueryHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WhenUnderQuota_ShouldReturnTrue()
    {
        // Arrange
        var quota = UserQuota.Create(ValidUserId, PlanType.Basic).Value;
        quota.Consume(5_000);

        _quotaRepositoryMock
            .Setup(r => r.GetTodayAsync(ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(quota);

        var query = new CheckTokenQuotaQuery(ValidUserId, 1_000); // 5000 + 1000 < 20000

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenOverQuota_ShouldReturnFalse()
    {
        // Arrange
        var quota = UserQuota.Create(ValidUserId, PlanType.Free).Value;
        quota.Consume(2_500);

        _quotaRepositoryMock
            .Setup(r => r.GetTodayAsync(ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(quota);

        var query = new CheckTokenQuotaQuery(ValidUserId, 600); // 2500 + 600 > 3000

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenNoQuotaExists_ShouldCreateQuotaAndReturnResult()
    {
        // Arrange: 오늘 쿼터 없음
        _quotaRepositoryMock
            .Setup(r => r.GetTodayAsync(ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserQuota?)null);

        _quotaRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var user = Domain.Identity.User.Create(ValidUserId, "test@example.com").Value;
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var query = new CheckTokenQuotaQuery(ValidUserId, 1_000);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue(); // Free 한도 3000, 요청 1000
        _quotaRepositoryMock.Verify(r => r.Add(It.IsAny<UserQuota>()), Times.Once);
        _quotaRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        _quotaRepositoryMock
            .Setup(r => r.GetTodayAsync(ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserQuota?)null);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Identity.User?)null);

        var query = new CheckTokenQuotaQuery(ValidUserId, 1_000);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
