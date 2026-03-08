namespace RianFriends.Domain.Identity;

/// <summary>사용자 역할</summary>
public enum UserRole
{
    /// <summary>일반 사용자 (본인 데이터만 접근)</summary>
    User,

    /// <summary>관리자 (전체 데이터 조회, 시스템 설정)</summary>
    Admin
}
