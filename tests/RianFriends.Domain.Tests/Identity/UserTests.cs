using FluentAssertions;
using RianFriends.Domain.Identity;
using RianFriends.Domain.Identity.Events;

namespace RianFriends.Domain.Tests.Identity;

[Trait("Category", "Unit")]
public class UserTests
{
    private static readonly Guid ValidId = Guid.NewGuid();
    private const string ValidEmail = "test@example.com";

    // ── Create ────────────────────────────────────────────

    [Fact]
    public void Create_WithValidEmail_ShouldSucceed()
    {
        var result = User.Create(ValidId, ValidEmail);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(ValidId);
        result.Value.Email.Should().Be(ValidEmail);
        result.Value.Plan.Should().Be(PlanType.Free);
        result.Value.Role.Should().Be(UserRole.User);
        result.Value.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyEmail_WhenNotHidden_ShouldFail()
    {
        var result = User.Create(ValidId, string.Empty, isEmailHidden: false);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyEmail_WhenIsEmailHidden_ShouldSucceed()
    {
        // Apple Sign In은 이메일을 숨길 수 있음
        var result = User.Create(ValidId, string.Empty, isEmailHidden: true);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsEmailHidden.Should().BeTrue();
    }

    // ── UpdateProfile ─────────────────────────────────────

    [Fact]
    public void UpdateProfile_WithValidData_ShouldSucceed()
    {
        var user = User.Create(ValidId, ValidEmail).Value;
        var birthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-20));

        var result = user.UpdateProfile("리안", birthDate, "KR");

        result.IsSuccess.Should().BeTrue();
        user.Nickname.Should().Be("리안");
        user.BirthDate.Should().Be(birthDate);
        user.CountryCode.Should().Be("KR");
    }

    [Fact]
    public void UpdateProfile_WithEmptyNickname_ShouldFail()
    {
        var user = User.Create(ValidId, ValidEmail).Value;
        var birthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-20));

        var result = user.UpdateProfile(string.Empty, birthDate, "KR");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void UpdateProfile_Under13Years_ShouldFail()
    {
        var user = User.Create(ValidId, ValidEmail).Value;
        var underageDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-12));

        var result = user.UpdateProfile("리안", underageDate, "KR");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("13세");
    }

    // ── Delete ────────────────────────────────────────────

    [Fact]
    public void Delete_ActiveAccount_ShouldSucceed()
    {
        var user = User.Create(ValidId, ValidEmail).Value;

        var result = user.Delete();

        result.IsSuccess.Should().BeTrue();
        user.DeletedAt.Should().NotBeNull();
        user.Email.Should().StartWith("deleted_");
        user.Nickname.Should().BeNull();
    }

    [Fact]
    public void Delete_AlreadyDeletedAccount_ShouldFail()
    {
        var user = User.Create(ValidId, ValidEmail).Value;
        user.Delete();

        var result = user.Delete();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Delete_ShouldRaiseDomainEvent()
    {
        var user = User.Create(ValidId, ValidEmail).Value;

        user.Delete();

        user.DomainEvents.Should().ContainSingle(e => e is UserDeletedEvent);
    }

    // ── UpdatePlan ────────────────────────────────────────

    [Fact]
    public void UpdatePlan_ToDifferentPlan_ShouldRaiseDomainEvent()
    {
        var user = User.Create(ValidId, ValidEmail).Value;

        user.UpdatePlan(PlanType.Basic);

        user.Plan.Should().Be(PlanType.Basic);
        user.DomainEvents.Should().ContainSingle(e => e is UserPlanChangedEvent);
    }

    [Fact]
    public void UpdatePlan_ToSamePlan_ShouldNotRaiseEvent()
    {
        var user = User.Create(ValidId, ValidEmail).Value;

        user.UpdatePlan(PlanType.Free); // 현재 플랜과 동일

        user.DomainEvents.Should().BeEmpty();
    }
}
