namespace RianFriends.Application.Identity.Interfaces;

/// <summary>
/// 자체 JWT 발급 인터페이스. Supabase JWT 대신 .NET API가 직접 발급합니다.
/// 비유: 놀이공원 입장권 발급기 — Supabase는 신분증 확인만, 입장권은 우리가 직접 발급.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>AccessToken을 생성합니다.</summary>
    /// <param name="userId">사용자 ID</param>
    /// <param name="email">사용자 이메일</param>
    /// <param name="role">사용자 역할 (user, admin)</param>
    string GenerateAccessToken(Guid userId, string email, string role);

    /// <summary>RefreshToken 원문 문자열을 생성합니다.</summary>
    string GenerateRefreshToken();

    /// <summary>RefreshToken 문자열을 SHA256 해시로 변환합니다.</summary>
    string HashToken(string token);
}
