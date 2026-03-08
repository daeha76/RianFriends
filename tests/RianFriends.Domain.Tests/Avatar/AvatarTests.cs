using FluentAssertions;
using RianFriends.Domain.Avatar;
using RianFriends.Domain.Avatar.Events;

namespace RianFriends.Domain.Tests.Avatar;

[Trait("Category", "Unit")]
public class AvatarTests
{
    private static readonly Guid ValidFriendId = Guid.NewGuid();

    // ── Create ───────────────────────────────────────────────

    [Fact]
    public void Create_WithValidFriendId_ShouldSucceed()
    {
        var result = Domain.Avatar.Avatar.Create(ValidFriendId);

        result.IsSuccess.Should().BeTrue();
        result.Value.FriendId.Should().Be(ValidFriendId);
        result.Value.HungerLevel.Should().Be(0);
        result.Value.HungerStatus.Should().Be(HungerStatus.Satisfied);
    }

    [Fact]
    public void Create_WithEmptyFriendId_ShouldFail()
    {
        var result = Domain.Avatar.Avatar.Create(Guid.Empty);

        result.IsFailure.Should().BeTrue();
    }

    // ── Feed ─────────────────────────────────────────────────

    [Fact]
    public void Feed_FromHungryState_ShouldDecreaseHungerLevel()
    {
        var avatar = Domain.Avatar.Avatar.Create(ValidFriendId).Value;
        // 배고픔을 먼저 올린 후 테스트
        avatar.IncreaseHunger(50);

        var result = avatar.Feed(20);

        result.IsSuccess.Should().BeTrue();
        avatar.HungerLevel.Should().Be(30);
    }

    [Fact]
    public void Feed_ShouldNotGoBelowZero()
    {
        var avatar = Domain.Avatar.Avatar.Create(ValidFriendId).Value;

        avatar.Feed(50); // HungerLevel은 이미 0이므로 0에서 멈춰야 함

        avatar.HungerLevel.Should().Be(0);
    }

    [Fact]
    public void Feed_ShouldRaiseAvatarFedEvent()
    {
        var avatar = Domain.Avatar.Avatar.Create(ValidFriendId).Value;
        avatar.IncreaseHunger(30);

        avatar.Feed(10);

        avatar.DomainEvents.Should().ContainSingle(e => e is AvatarFedEvent);
    }

    [Fact]
    public void Feed_WithZeroAmount_ShouldFail()
    {
        var avatar = Domain.Avatar.Avatar.Create(ValidFriendId).Value;

        var result = avatar.Feed(0);

        result.IsFailure.Should().BeTrue();
    }

    // ── IncreaseHunger ───────────────────────────────────────

    [Fact]
    public void IncreaseHunger_ShouldIncreaseHungerLevel()
    {
        var avatar = Domain.Avatar.Avatar.Create(ValidFriendId).Value;

        avatar.IncreaseHunger(50); // 50 >= 40이므로 Hungry

        avatar.HungerLevel.Should().Be(50);
        avatar.HungerStatus.Should().Be(HungerStatus.Hungry);
    }

    [Fact]
    public void IncreaseHunger_ShouldNotExceed100()
    {
        var avatar = Domain.Avatar.Avatar.Create(ValidFriendId).Value;
        avatar.IncreaseHunger(90);

        avatar.IncreaseHunger(50); // 140이 되어야 하지만 100으로 제한

        avatar.HungerLevel.Should().Be(100);
        avatar.HungerStatus.Should().Be(HungerStatus.Starving);
    }

    [Fact]
    public void IncreaseHunger_When70Threshold_ShouldRaiseAvatarHungryEvent()
    {
        var avatar = Domain.Avatar.Avatar.Create(ValidFriendId).Value;
        avatar.IncreaseHunger(65); // 65로 올림

        avatar.IncreaseHunger(10); // 75로 → 임계치 초과

        avatar.DomainEvents.Should().ContainSingle(e => e is AvatarHungryEvent);
        avatar.HungerLevel.Should().Be(75);
    }

    [Fact]
    public void IncreaseHunger_AlreadyAbove70_ShouldNotRaiseEventAgain()
    {
        var avatar = Domain.Avatar.Avatar.Create(ValidFriendId).Value;
        avatar.IncreaseHunger(80); // 이미 70 이상

        avatar.IncreaseHunger(10); // 90 → 이미 Starving이므로 이벤트 추가 발행 없음

        // 첫 번째 IncreaseHunger에서만 이벤트 발행 (80 >= 70)
        avatar.DomainEvents.Count(e => e is AvatarHungryEvent).Should().Be(1);
    }

    // ── HungerStatus ─────────────────────────────────────────

    [Theory]
    [InlineData(0, HungerStatus.Satisfied)]
    [InlineData(39, HungerStatus.Satisfied)]
    [InlineData(40, HungerStatus.Hungry)]
    [InlineData(69, HungerStatus.Hungry)]
    [InlineData(70, HungerStatus.Starving)]
    [InlineData(100, HungerStatus.Starving)]
    public void HungerStatus_ShouldMatchLevel(int level, HungerStatus expectedStatus)
    {
        var avatar = Domain.Avatar.Avatar.Create(ValidFriendId).Value;
        if (level > 0)
        {
            avatar.IncreaseHunger(level);
        }

        avatar.HungerStatus.Should().Be(expectedStatus);
    }
}
