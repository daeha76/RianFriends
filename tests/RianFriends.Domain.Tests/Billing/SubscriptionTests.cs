using FluentAssertions;
using RianFriends.Domain.Billing;
using RianFriends.Domain.Billing.Events;
using RianFriends.Domain.Identity;

namespace RianFriends.Domain.Tests.Billing;

[Trait("Category", "Unit")]
public class SubscriptionTests
{
    private static readonly Guid ValidUserId = Guid.NewGuid();
    private const string ValidRcId = "rc_customer_123";
    private const string ValidProductId = "com.rianfriends.basic_monthly";

    // ── Create ─────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ShouldSucceedAndBeActive()
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMonths(1);

        var result = Subscription.Create(ValidUserId, ValidRcId, ValidProductId, PlanType.Basic, expiresAt);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsActive.Should().BeTrue();
        result.Value.UserId.Should().Be(ValidUserId);
        result.Value.PlanType.Should().Be(PlanType.Basic);
    }

    [Fact]
    public void Create_ShouldRaiseSubscriptionActivatedEvent()
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMonths(1);

        var result = Subscription.Create(ValidUserId, ValidRcId, ValidProductId, PlanType.Basic, expiresAt);

        result.Value.DomainEvents.Should().ContainSingle(e => e is SubscriptionActivatedEvent);
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldFail()
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMonths(1);

        var result = Subscription.Create(Guid.Empty, ValidRcId, ValidProductId, PlanType.Basic, expiresAt);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_WithPastExpiresAt_ShouldFail()
    {
        var pastDate = DateTimeOffset.UtcNow.AddDays(-1);

        var result = Subscription.Create(ValidUserId, ValidRcId, ValidProductId, PlanType.Basic, pastDate);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyRevenueCatId_ShouldFail()
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMonths(1);

        var result = Subscription.Create(ValidUserId, string.Empty, ValidProductId, PlanType.Basic, expiresAt);

        result.IsFailure.Should().BeTrue();
    }

    // ── Deactivate ─────────────────────────────────────────

    [Fact]
    public void Deactivate_ActiveSubscription_ShouldSetIsActiveFalse()
    {
        var subscription = Subscription.Create(
            ValidUserId, ValidRcId, ValidProductId, PlanType.Basic, DateTimeOffset.UtcNow.AddMonths(1)).Value;
        subscription.ClearDomainEvents();

        var result = subscription.Deactivate();

        result.IsSuccess.Should().BeTrue();
        subscription.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ShouldRaiseSubscriptionExpiredEvent()
    {
        var subscription = Subscription.Create(
            ValidUserId, ValidRcId, ValidProductId, PlanType.Basic, DateTimeOffset.UtcNow.AddMonths(1)).Value;
        subscription.ClearDomainEvents();

        subscription.Deactivate();

        subscription.DomainEvents.Should().ContainSingle(e => e is SubscriptionExpiredEvent);
    }

    [Fact]
    public void Deactivate_AlreadyDeactivatedSubscription_ShouldFail()
    {
        var subscription = Subscription.Create(
            ValidUserId, ValidRcId, ValidProductId, PlanType.Basic, DateTimeOffset.UtcNow.AddMonths(1)).Value;
        subscription.Deactivate();

        var result = subscription.Deactivate(); // 중복 비활성화

        result.IsFailure.Should().BeTrue();
    }
}
