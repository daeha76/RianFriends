using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Identity.Dtos;
using RianFriends.Application.Identity.Interfaces;
using Supabase;

namespace RianFriends.Infrastructure.Identity;

/// <summary>
/// Supabase Auth 연동 서비스 구현체 (supabase-csharp 0.16.x).
/// 소셜 토큰 검증 + 사용자 정보 반환만 담당. JWT 발급은 JwtTokenService가 처리합니다.
/// </summary>
internal sealed class SupabaseAuthService : IAuthService
{
    private readonly Client _supabase;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SupabaseAuthService> _logger;

    /// <summary>의존성을 주입합니다.</summary>
    public SupabaseAuthService(
        Client supabase,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<SupabaseAuthService> logger)
    {
        _supabase = supabase;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<SocialUserInfo> SignInAsync(string email, string password, CancellationToken ct = default)
    {
        var session = await _supabase.Auth.SignIn(email, password);
        if (session?.User is null)
        {
            throw new InvalidOperationException("Supabase 로그인 실패: 사용자 정보를 받을 수 없습니다.");
        }

        _logger.LogInformation("Supabase 이메일 인증 성공. Email: {Email}", email);
        return MapToUserInfo(session);
    }

    /// <inheritdoc />
    public async Task<SocialUserInfo> SignInWithOAuthAsync(string provider, string accessToken, CancellationToken ct = default)
    {
        var session = await _supabase.Auth.SignInWithIdToken(
            MapProvider(provider),
            accessToken);

        if (session?.User is null)
        {
            throw new InvalidOperationException($"Supabase OAuth 인증 실패. Provider: {provider}");
        }

        _logger.LogInformation("Supabase OAuth 인증 성공. Provider: {Provider}", provider);
        return MapToUserInfo(session);
    }

    /// <inheritdoc />
    public async Task DeleteUserAsync(Guid userId, CancellationToken ct = default)
    {
        var serviceKey = _configuration["Supabase:ServiceKey"]
            ?? throw new InvalidOperationException("Supabase:ServiceKey가 설정되지 않았습니다.");

        var supabaseUrl = _configuration["Supabase:Url"]
            ?? throw new InvalidOperationException("Supabase:Url이 설정되지 않았습니다.");

        using var httpClient = _httpClientFactory.CreateClient();
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

    private static SocialUserInfo MapToUserInfo(Supabase.Gotrue.Session session)
        => new(
            session.User?.Id is { } id ? Guid.Parse(id) : Guid.Empty,
            session.User?.Email ?? string.Empty);
}
