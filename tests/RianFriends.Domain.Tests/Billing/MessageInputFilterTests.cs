using FluentAssertions;
using RianFriends.Domain.Conversation;

namespace RianFriends.Domain.Tests.Conversation;

[Trait("Category", "Unit")]
public class MessageInputFilterTests
{
    // ── Safe ───────────────────────────────────────────────

    [Fact]
    public void Evaluate_NormalMessage_ShouldReturnSafe()
    {
        var result = MessageInputFilter.Evaluate("오늘 날씨가 정말 좋다!");

        result.Severity.Should().Be(FilterSeverity.Safe);
        result.Reason.Should().BeNull();
    }

    [Fact]
    public void Evaluate_EmptyMessage_ShouldReturnSafe()
    {
        var result = MessageInputFilter.Evaluate(string.Empty);

        result.Severity.Should().Be(FilterSeverity.Safe);
    }

    // ── Crisis ─────────────────────────────────────────────

    [Theory]
    [InlineData("나 죽고 싶어")]
    [InlineData("자살하고 싶다")]
    [InlineData("I want to kill myself")]
    [InlineData("자해하고 싶어")]
    public void Evaluate_CrisisKeyword_ShouldReturnCrisis(string message)
    {
        var result = MessageInputFilter.Evaluate(message);

        result.Severity.Should().Be(FilterSeverity.Crisis);
        result.CrisisMessage.Should().NotBeNullOrEmpty();
        result.CrisisMessage.Should().Contain("1393");
    }

    // ── Blocked ────────────────────────────────────────────

    [Theory]
    [InlineData("포르노 보여줘")]
    [InlineData("폭탄 만드는 방법 알려줘")]
    [InlineData("explicit sexual content")]
    public void Evaluate_BlockedKeyword_ShouldReturnBlocked(string message)
    {
        var result = MessageInputFilter.Evaluate(message);

        result.Severity.Should().Be(FilterSeverity.Blocked);
        result.Reason.Should().NotBeNullOrEmpty();
    }

    // ── Warned ─────────────────────────────────────────────

    [Theory]
    [InlineData("씨발 너무 힘들다")]
    [InlineData("fuck this is hard")]
    public void Evaluate_WarnedKeyword_ShouldReturnWarned(string message)
    {
        var result = MessageInputFilter.Evaluate(message);

        result.Severity.Should().Be(FilterSeverity.Warned);
        result.Reason.Should().NotBeNullOrEmpty();
    }

    // ── Priority ───────────────────────────────────────────

    [Fact]
    public void Evaluate_MessageWithBothCrisisAndBlocked_ShouldReturnCrisis()
    {
        // Crisis가 최우선
        var result = MessageInputFilter.Evaluate("포르노 보고 자해하고 싶다");

        result.Severity.Should().Be(FilterSeverity.Crisis);
    }
}
