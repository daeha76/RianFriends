using RianFriends.Application.Identity.Dtos;

namespace RianFriends.Application.Identity.Interfaces;

/// <summary>
/// Supabase Auth 연동 인터페이스.
/// 소셜 토큰 검증 + 사용자 정보 반환만 담당. JWT 발급은 IJwtTokenService가 처리합니다.
/// </summary>
public interface IAuthService
{
    /// <summary>이메일/비밀번호로 인증하고 사용자 정보를 반환합니다.</summary>
    Task<SocialUserInfo> SignInAsync(string email, string password, CancellationToken ct = default);

    /// <summary>소셜 OAuth 토큰으로 인증하고 사용자 정보를 반환합니다.</summary>
    Task<SocialUserInfo> SignInWithOAuthAsync(string provider, string accessToken, CancellationToken ct = default);

    /// <summary>Supabase Auth 계정을 삭제합니다.</summary>
    Task DeleteUserAsync(Guid userId, CancellationToken ct = default);
}
