using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Identity.Dtos;
using RianFriends.Application.Identity.Interfaces;
using Supabase;

namespace RianFriends.Infrastructure.Identity;

/// <summary>Supabase Auth 연동 서비스 구현체 (supabase-csharp 0.16.x)</summary>
internal sealed class SupabaseAuthService : IAuthService
{
    private readonly Client _supabase;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SupabaseAuthService> _logger;

    /// <summary>의존성을 주입합니다.</summary>
    public SupabaseAuthService(
        Client supabase,
        IConfiguration configuration,
        ILogger<SupabaseAuthService> logger)
    {
        _supabase = supabase;
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AuthResultDto> SignInAsync(string email, string password, CancellationToken ct = default)
    {
        var session = await _supabase.Auth.SignIn(email, password);
        if (session?.AccessToken is null)
        {
            throw new InvalidOperationException("Supabase 로그인 실패: 세션을 받을 수 없습니다.");
        }

        _logger.LogInformation("Supabase 이메일 로그인 성공. Email: {Email}", email);
        return MapToDto(session);
    }

    /// <inheritdoc />
    public async Task<AuthResultDto> SignInWithOAuthAsync(string provider, string accessToken, CancellationToken ct = default)
    {
        // supabase-csharp 0.16.x: SignInWithIdToken으로 소셜 토큰 교환
        // MAUI 앱에서 소셜 SDK로 획득한 id_token을 Supabase에 전달
        var session = await _supabase.Auth.SignInWithIdToken(
            MapProvider(provider),
            accessToken);

        if (session?.AccessToken is null)
        {
            throw new InvalidOperationException($"Supabase OAuth 로그인 실패. Provider: {provider}");
        }

        _logger.LogInformation("Supabase OAuth 로그인 성공. Provider: {Provider}", provider);
        return MapToDto(session);
    }

    /// <inheritdoc />
    public async Task<AuthResultDto> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var session = await _supabase.Auth.RefreshSession();
        if (session?.AccessToken is null)
        {
            throw new InvalidOperationException("토큰 갱신 실패: 새 세션을 받을 수 없습니다.");
        }

        return MapToDto(session);
    }

    /// <inheritdoc />
    public async Task DeleteUserAsync(Guid userId, CancellationToken ct = default)
    {
        // Admin 작업은 서비스 키로 별도 HTTP 호출
        // supabase-csharp 0.16.x에서는 Admin API가 별도 클라이언트를 통해 접근
        var serviceKey = _configuration["Supabase:ServiceKey"]
            ?? throw new InvalidOperationException("Supabase:ServiceKey가 설정되지 않았습니다.");

        var supabaseUrl = _configuration["Supabase:Url"]
            ?? throw new InvalidOperationException("Supabase:Url이 설정되지 않았습니다.");

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("apikey", serviceKey);
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {serviceKey}");

        var response = await httpClient.DeleteAsync(
            $"{supabaseUrl}/auth/v1/admin/users/{userId}", ct);

        response.EnsureSuccessStatusCode();
        _logger.LogInformation("Supabase Auth 계정 삭제 완료. UserId: {UserId}", userId);
    }

    private static Supabase.Gotrue.Constants.Provider MapProvider(string provider)
        => provider.ToLowerInvariant() switch
        {
            "google" => Supabase.Gotrue.Constants.Provider.Google,
            "apple" => Supabase.Gotrue.Constants.Provider.Apple,
            "kakao" => Supabase.Gotrue.Constants.Provider.Kakao,
            _ => throw new ArgumentException($"지원하지 않는 프로바이더: {provider}", nameof(provider))
        };

    private static AuthResultDto MapToDto(Supabase.Gotrue.Session session)
        => new(
            session.User?.Id is { } id ? Guid.Parse(id) : Guid.Empty,
            session.User?.Email ?? string.Empty,
            session.AccessToken!,
            session.RefreshToken ?? string.Empty,
            session.ExpiresAt());
}
