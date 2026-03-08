using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RianFriends.Application.Abstractions;
using RianFriends.Application.Billing.Commands.HandleRevenueCatWebhook;
using RianFriends.Application.Identity.Interfaces;
using RianFriends.Domain.Billing;
using RianFriends.Domain.Identity;

namespace RianFriends.Application.Tests.Billing;

[Trait("Category", "Unit")]
public class HandleRevenueCatWebhookCommandHandlerTests
{
    private readonly Mock<IBillingService> _billingServiceMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<ISubscriptionRepository> _subscriptionRepositoryMock = new();
    private readonly Mock<IUserQuotaRepository> _quotaRepositoryMock = new();
    private readonly HandleRevenueCatWebhookCommandHandler _sut;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private const string ValidRawJson = "{\"event\":{}}";

    public HandleRevenueCatWebhookCommandHandlerTests()
    {
        _sut = new HandleRevenueCatWebhookCommandHandler(
            _billingServiceMock.Object,
            _userRepositoryMock.Object,
            _subscriptionRepositoryMock.Object,
            _quotaRepositoryMock.Object,
            NullLogger<HandleRevenueCatWebhookCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WhenInitialPurchase_ShouldUpdateUserPlanAndCreateSubscription()
    {
        // Arrange
        var payload = new RevenueCatWebhookPayload
        {
            EventType = "INITIAL_PURCHASE",
            AppUserId = ValidUserId.ToString(),
            RevenueCatId = "rc_123",
            ProductId = "com.rianfriends.basic_monthly",
            ExpirationAtMs = DateTimeOffset.UtcNow.AddMonths(1).ToUnixTimeMilliseconds()
        };

        _billingServiceMock.Setup(s => s.ParseWebhook(ValidRawJson)).Returns(payload);
        _billingServiceMock.Setup(s => s.ParseEventType("INITIAL_PURCHASE")).Returns(RevenueCatEventType.InitialPurchase);
        _billingServiceMock.Setup(s => s.MapProductToPlan("com.rianfriends.basic_monthly")).Returns(PlanType.Basic);

        var user = Domain.Identity.User.Create(ValidUserId, "test@example.com").Value;
        _userRepositoryMock.Setup(r => r.GetByIdAsync(ValidUserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _subscriptionRepositoryMock
            .Setup(r => r.GetActiveByUserIdAsync(ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);
        _subscriptionRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _quotaRepositoryMock
            .Setup(r => r.GetTodayAsync(ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserQuota?)null);

        var command = new HandleRevenueCatWebhookCommand(ValidRawJson, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Plan.Should().Be(PlanType.Basic);
        _subscriptionRepositoryMock.Verify(r => r.Add(It.IsAny<Subscription>()), Times.Once);
        _userRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenExpiration_ShouldDeactivateAndDowngradePlanToFree()
    {
        // Arrange
        var payload = new RevenueCatWebhookPayload
        {
            EventType = "EXPIRATION",
            AppUserId = ValidUserId.ToString(),
            RevenueCatId = "rc_123",
            ProductId = "com.rianfriends.basic_monthly"
        };

        _billingServiceMock.Setup(s => s.ParseWebhook(ValidRawJson)).Returns(payload);
        _billingServiceMock.Setup(s => s.ParseEventType("EXPIRATION")).Returns(RevenueCatEventType.Expiration);

        var user = Domain.Identity.User.Create(ValidUserId, "test@example.com").Value;
        user.UpdatePlan(PlanType.Basic); // 기존 Basic 플랜
        _userRepositoryMock.Setup(r => r.GetByIdAsync(ValidUserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var activeSubscription = Subscription.Create(
            ValidUserId, "rc_123", "com.rianfriends.basic_monthly", PlanType.Basic,
            DateTimeOffset.UtcNow.AddMonths(1)).Value;
        _subscriptionRepositoryMock
            .Setup(r => r.GetActiveByUserIdAsync(ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeSubscription);
        _subscriptionRepositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new HandleRevenueCatWebhookCommand(ValidRawJson, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Plan.Should().Be(PlanType.Free);
        activeSubscription.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenWebhookParsingFails_ShouldReturnFailure()
    {
        _billingServiceMock.Setup(s => s.ParseWebhook(It.IsAny<string>())).Returns((RevenueCatWebhookPayload?)null);

        var command = new HandleRevenueCatWebhookCommand("invalid_json", null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
