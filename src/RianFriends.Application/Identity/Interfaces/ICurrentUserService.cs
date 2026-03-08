namespace RianFriends.Application.Identity.Interfaces;

/// <summary>현재 인증된 사용자 컨텍스트</summary>
public interface ICurrentUserService
{
    /// <summary>현재 사용자 ID (JWT sub 클레임)</summary>
    Guid UserId { get; }

    /// <summary>현재 사용자 역할</summary>
    string Role { get; }

    /// <summary>관리자 여부</summary>
    bool IsAdmin { get; }
}
