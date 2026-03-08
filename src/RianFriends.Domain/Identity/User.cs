using RianFriends.Domain.Common;
using RianFriends.Domain.Identity.Events;

namespace RianFriends.Domain.Identity;

/// <summary>RianFriends 사용자 엔티티</summary>
public sealed class User : AuditableEntity
{
    /// <summary>이메일 주소 (Supabase Auth와 동기화)</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>닉네임 (앱 내 표시 이름)</summary>
    public string? Nickname { get; private set; }

    /// <summary>생년월일 (만 13세 미만 가입 차단에 사용)</summary>
    public DateOnly? BirthDate { get; private set; }

    /// <summary>국가 코드 (예: KR, US)</summary>
    public string? CountryCode { get; private set; }

    /// <summary>구독 플랜</summary>
    public PlanType Plan { get; private set; } = PlanType.Free;

    /// <summary>사용자 역할</summary>
    public UserRole Role { get; private set; } = UserRole.User;

    /// <summary>Apple Sign In 이메일 숨기기 여부</summary>
    public bool IsEmailHidden { get; private set; }

    /// <summary>탈퇴 일시 (null이면 활성 계정)</summary>
    public DateTimeOffset? DeletedAt { get; private set; }

    /// <summary>EF Core용 기본 생성자</summary>
    private User() { }

    /// <summary>새 사용자를 생성합니다.</summary>
    /// <param name="id">Supabase Auth user.id와 동일해야 합니다.</param>
    /// <param name="email">이메일 주소</param>
    /// <param name="isEmailHidden">Apple Sign In 이메일 숨기기 여부</param>
    public static Result<User> Create(Guid id, string email, bool isEmailHidden = false)
    {
        if (string.IsNullOrWhiteSpace(email) && !isEmailHidden)
        {
            return Result.Failure<User>("이메일은 필수입니다.");
        }

        var user = new User
        {
            Id = id,
            Email = email,
            IsEmailHidden = isEmailHidden
        };

        return Result.Success(user);
    }

    /// <summary>프로필을 업데이트합니다.</summary>
    public Result UpdateProfile(string nickname, DateOnly birthDate, string countryCode)
    {
        if (string.IsNullOrWhiteSpace(nickname))
        {
            return Result.Failure("닉네임은 필수입니다.");
        }

        if (birthDate > DateOnly.FromDateTime(DateTime.Today.AddYears(-13)))
        {
            return Result.Failure("만 13세 미만은 서비스를 이용할 수 없습니다.");
        }

        Nickname = nickname;
        BirthDate = birthDate;
        CountryCode = countryCode;

        return Result.Success();
    }

    /// <summary>계정을 탈퇴 처리합니다 (Soft Delete).</summary>
    public Result Delete()
    {
        if (DeletedAt is not null)
        {
            return Result.Failure("이미 탈퇴한 계정입니다.");
        }

        Email = $"deleted_{Id}@deleted.com";
        Nickname = null;
        DeletedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new UserDeletedEvent(Id));
        return Result.Success();
    }

    /// <summary>플랜을 변경합니다 (RevenueCat Webhook에서 호출).</summary>
    public void UpdatePlan(PlanType newPlan)
    {
        if (Plan == newPlan)
        {
            return;
        }

        Plan = newPlan;
        AddDomainEvent(new UserPlanChangedEvent(Id, newPlan));
    }
}
