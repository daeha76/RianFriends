using FluentAssertions;
using RianFriends.Domain.Notification;

namespace RianFriends.Domain.Tests.Notification;

[Trait("Category", "Unit")]
public class WakeUpAlarmTests
{
    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidFriendId = Guid.NewGuid();
    private static readonly TimeOnly ValidTime = new(7, 0);

    // ── Create ───────────────────────────────────────────────

    [Fact]
    public void Create_WithValidArgs_ShouldSucceed()
    {
        var result = WakeUpAlarm.Create(ValidUserId, ValidFriendId, ValidTime);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(ValidUserId);
        result.Value.FriendId.Should().Be(ValidFriendId);
        result.Value.AlarmTime.Should().Be(ValidTime);
        result.Value.IsEnabled.Should().BeTrue();
        result.Value.RepeatDays.Should().Be(0);
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldFail()
    {
        var result = WakeUpAlarm.Create(Guid.Empty, ValidFriendId, ValidTime);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyFriendId_ShouldFail()
    {
        var result = WakeUpAlarm.Create(ValidUserId, Guid.Empty, ValidTime);

        result.IsFailure.Should().BeTrue();
    }

    // ── Toggle ───────────────────────────────────────────────

    [Fact]
    public void Toggle_EnabledAlarm_ShouldDisable()
    {
        var alarm = WakeUpAlarm.Create(ValidUserId, ValidFriendId, ValidTime).Value;

        alarm.Toggle();

        alarm.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public void Toggle_DisabledAlarm_ShouldEnable()
    {
        var alarm = WakeUpAlarm.Create(ValidUserId, ValidFriendId, ValidTime).Value;
        alarm.Toggle(); // 비활성화

        alarm.Toggle(); // 다시 활성화

        alarm.IsEnabled.Should().BeTrue();
    }

    // ── UpdateAlarm ──────────────────────────────────────────

    [Fact]
    public void UpdateAlarm_ShouldChangeTimeAndRepeatDays()
    {
        var alarm = WakeUpAlarm.Create(ValidUserId, ValidFriendId, ValidTime).Value;
        var newTime = new TimeOnly(8, 30);
        byte newRepeatDays = 0b0111110; // 월~금

        alarm.UpdateAlarm(newTime, newRepeatDays);

        alarm.AlarmTime.Should().Be(newTime);
        alarm.RepeatDays.Should().Be(newRepeatDays);
    }

    // ── RepeatDays ───────────────────────────────────────────

    [Fact]
    public void Create_WithRepeatDays_ShouldStoreCorrectly()
    {
        byte allDays = 0b1111111; // 매일 반복

        var alarm = WakeUpAlarm.Create(ValidUserId, ValidFriendId, ValidTime, allDays).Value;

        alarm.RepeatDays.Should().Be(allDays);
    }
}
