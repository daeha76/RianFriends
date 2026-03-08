using FluentAssertions;
using RianFriends.Domain.Billing;
using RianFriends.Domain.Billing.Events;
using RianFriends.Domain.Identity;

namespace RianFriends.Domain.Tests.Billing;

[Trait("Category", "Unit")]
public class UserQuotaTests
{
    private static readonly Guid ValidUserId = Guid.NewGuid();

    // ── Create ─────────────────────────────────────────────

    [Fact]
    public void Create_FreePlan_ShouldSetQuotaTo3000()
    {
        var result = UserQuota.Create(ValidUserId, PlanType.Free);

        result.IsSuccess.Should().BeTrue();
        result.Value.QuotaLimit.Should().Be(3_000);
        result.Value.UsedTokens.Should().Be(0);
    }

    [Fact]
    public void Create_BasicPlan_ShouldSetQuotaTo20000()
    {
        var result = UserQuota.Create(ValidUserId, PlanType.Basic);

        result.IsSuccess.Should().BeTrue();
        result.Value.QuotaLimit.Should().Be(20_000);
    }

    [Fact]
    public void Create_ProPlan_ShouldSetQuotaToMaxValue()
    {
        var result = UserQuota.Create(ValidUserId, PlanType.Pro);

        result.IsSuccess.Should().BeTrue();
        result.Value.QuotaLimit.Should().Be(int.MaxValue);
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldFail()
    {
        var result = UserQuota.Create(Guid.Empty, PlanType.Free);

        result.IsFailure.Should().BeTrue();
    }

    // ── Consume ────────────────────────────────────────────

    [Fact]
    public void Consume_WhenUnderLimit_ShouldSucceed()
    {
        var quota = UserQuota.Create(ValidUserId, PlanType.Free).Value;

        var result = quota.Consume(1_000);

        result.IsSuccess.Should().BeTrue();
        quota.UsedTokens.Should().Be(1_000);
    }

    [Fact]
    public void Consume_WhenExactlyAtLimit_ShouldSucceed()
    {
        var quota = UserQuota.Create(ValidUserId, PlanType.Free).Value;

        var result = quota.Consume(3_000);

        result.IsSuccess.Should().BeTrue();
        quota.UsedTokens.Should().Be(3_000);
    }

    [Fact]
    public void Consume_WhenOverLimit_ShouldFail()
    {
        var quota = UserQuota.Create(ValidUserId, PlanType.Free).Value;
        quota.Consume(2_500);

        var result = quota.Consume(600); // 2500 + 600 > 3000

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("한도");
        quota.UsedTokens.Should().Be(2_500); // 실패 시 변경 없음
    }

    [Fact]
    public void Consume_WithZeroOrNegative_ShouldFail()
    {
        var quota = UserQuota.Create(ValidUserId, PlanType.Free).Value;

        var result = quota.Consume(0);

        result.IsFailure.Should().BeTrue();
    }

    // ── Reset ──────────────────────────────────────────────

    [Fact]
    public void Reset_ShouldClearUsedTokens()
    {
        var quota = UserQuota.Create(ValidUserId, PlanType.Free).Value;
        quota.Consume(1_500);

        quota.Reset();

        quota.UsedTokens.Should().Be(0);
    }

    // ── UpdateLimit ────────────────────────────────────────

    [Fact]
    public void UpdateLimit_WhenUpgradeToPro_ShouldIncreaseQuota()
    {
        var quota = UserQuota.Create(ValidUserId, PlanType.Free).Value;

        quota.UpdateLimit(PlanType.Pro);

        quota.QuotaLimit.Should().Be(int.MaxValue);
    }
}
