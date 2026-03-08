using FluentAssertions;
using RianFriends.Domain.Common;

namespace RianFriends.Domain.Tests.Common;

[Trait("Category", "Unit")]
public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessResult()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void Failure_ShouldCreateFailureResult()
    {
        var result = Result.Failure("오류가 발생했습니다.");

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("오류가 발생했습니다.");
    }

    [Fact]
    public void SuccessT_ShouldContainValue()
    {
        var result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void FailureT_ShouldNotExposeValue()
    {
        var result = Result.Failure<int>("실패");

        result.IsFailure.Should().BeTrue();
        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Failure_WithEmptyError_ShouldThrow()
    {
        var act = () => Result.Failure(string.Empty);

        act.Should().Throw<InvalidOperationException>();
    }
}
