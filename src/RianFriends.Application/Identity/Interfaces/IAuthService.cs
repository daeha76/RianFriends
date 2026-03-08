using RianFriends.Application.Identity.Dtos;

namespace RianFriends.Application.Identity.Interfaces;

/// <summary>Supabase Auth 연동 인터페이스</summary>
public interface IAuthService
{
    /// <summary>이메일/비밀번호로 로그인합니다.</summary>
    Task<AuthResultDto> SignInAsync(string email, string password, CancellationToken ct = default);

    /// <summary>소셜 OAuth 토큰으로 로그인합니다.</summary>
    Task<AuthResultDto> SignInWithOAuthAsync(string provider, string accessToken, CancellationToken ct = default);

    /// <summary>RefreshToken으로 AccessToken을 갱신합니다.</summary>
    Task<AuthResultDto> RefreshAsync(string refreshToken, CancellationToken ct = default);

    /// <summary>Supabase Auth 계정을 삭제합니다.</summary>
    Task DeleteUserAsync(Guid userId, CancellationToken ct = default);
}
