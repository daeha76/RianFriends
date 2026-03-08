using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Identity.Dtos;
using RianFriends.Application.Identity.Interfaces;
using RianFriends.Domain.Common;
using RianFriends.Domain.Identity;

namespace RianFriends.Application.Identity.Commands.Login;

/// <summary>
/// 로그인 Command 핸들러.
/// Supabase Auth로 소셜 토큰 검증 → 자체 JWT 발급 → RefreshToken DB 저장.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResultDto>>
{
    private readonly IAuthService _authService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LoginCommandHandler> _logger;

    /// <summary>의존성을 주입합니다.</summary>
    public LoginCommandHandler(
        IAuthService authService,
        IJwtTokenService jwtTokenService,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IConfiguration configuration,
        ILogger<LoginCommandHandler> logger)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>로그인을 처리합니다. Supabase 검증 → 자체 JWT 발급 → DB 동기화.</summary>
    public async Task<Result<AuthResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Supabase Auth로 소셜 토큰 검증 (JWT 발급 없이 사용자 정보만 획득)
        SocialUserInfo userInfo;
        if (request.Provider == "email")
        {
            userInfo = await _authService.SignInAsync(request.Email!, request.Credential, cancellationToken);
        }
        else
        {
            userInfo = await _authService.SignInWithOAuthAsync(request.Provider, request.Credential, cancellationToken);
        }

        // 2. 최초 로그인 시 DB에 사용자 레코드 자동 생성 (Upsert)
        var user = await _userRepository.GetByIdAsync(userInfo.UserId, cancellationToken);
        string role;
        if (user is null)
        {
            var createResult = User.Create(userInfo.UserId, userInfo.Email, request.IsEmailHidden);
            if (createResult.IsFailure)
            {
                return Result.Failure<AuthResultDto>(createResult.Error);
            }

            _userRepository.Add(createResult.Value);
            role = "user";
            _logger.LogInformation("신규 사용자 DB 등록 완료. UserId: {UserId}", userInfo.UserId);
        }
        else
        {
            role = user.Role.ToString().ToLowerInvariant();
        }

        // 3. 자체 JWT AccessToken 발급
        var accessToken = _jwtTokenService.GenerateAccessToken(userInfo.UserId, userInfo.Email, role);

        // 4. RefreshToken 생성 → DB 저장
        var refreshTokenExpirationDays = int.TryParse(
            _configuration["Jwt:RefreshTokenExpirationDays"], out var days) ? days : 30;
        var refreshTokenRaw = _jwtTokenService.GenerateRefreshToken();
        var refreshTokenHash = _jwtTokenService.HashToken(refreshTokenRaw);

        var refreshTokenEntity = RefreshToken.Create(userInfo.UserId, refreshTokenHash, refreshTokenExpirationDays);
        _refreshTokenRepository.Add(refreshTokenEntity);

        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        // 5. AccessToken 만료 시간 계산
        var accessTokenExpirationMinutes = int.TryParse(
            _configuration["Jwt:AccessTokenExpirationMinutes"], out var min) ? min : 60;
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(accessTokenExpirationMinutes);

        _logger.LogInformation("사용자 로그인 성공. Provider: {Provider}, UserId: {UserId}", request.Provider, userInfo.UserId);

        return Result.Success(new AuthResultDto(
            userInfo.UserId,
            userInfo.Email,
            accessToken,
            refreshTokenRaw,
            expiresAt));
    }
}
