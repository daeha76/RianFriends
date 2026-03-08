using MediatR;
using Microsoft.Extensions.Configuration;
using RianFriends.Application.Identity.Dtos;
using RianFriends.Application.Identity.Interfaces;
using RianFriends.Domain.Common;
using RefreshTokenEntity = RianFriends.Domain.Identity.RefreshToken;

namespace RianFriends.Application.Identity.Commands.RefreshToken;

/// <summary>
/// AccessToken 갱신 Command 핸들러.
/// DB의 RefreshToken을 검증하고, 토큰 회전(rotation) 후 새 JWT 발급.
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResultDto>>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    /// <summary>의존성을 주입합니다.</summary>
    public RefreshTokenCommandHandler(
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IConfiguration configuration)
    {
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _configuration = configuration;
    }

    /// <summary>RefreshToken으로 새 AccessToken + RefreshToken을 발급합니다.</summary>
    public async Task<Result<AuthResultDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // 1. RefreshToken 해시로 DB 조회
        var tokenHash = _jwtTokenService.HashToken(request.RefreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (existingToken is null || !existingToken.IsActive)
        {
            return Result.Failure<AuthResultDto>("유효하지 않거나 만료된 RefreshToken입니다.");
        }

        // 2. 사용자 정보 조회
        var user = await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<AuthResultDto>("사용자를 찾을 수 없습니다.");
        }

        // 3. 기존 토큰 폐기 + 새 토큰 생성 (Rotation)
        var refreshTokenExpirationDays = int.TryParse(
            _configuration["Jwt:RefreshTokenExpirationDays"], out var days) ? days : 30;
        var newRefreshTokenRaw = _jwtTokenService.GenerateRefreshToken();
        var newRefreshTokenHash = _jwtTokenService.HashToken(newRefreshTokenRaw);

        var newRefreshToken = RefreshTokenEntity.Create(user.Id, newRefreshTokenHash, refreshTokenExpirationDays);
        existingToken.Revoke(newRefreshToken.Id);
        _refreshTokenRepository.Add(newRefreshToken);

        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        // 4. 새 AccessToken 발급
        var role = user.Role.ToString().ToLowerInvariant();
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email, role);

        var accessTokenExpirationMinutes = int.TryParse(
            _configuration["Jwt:AccessTokenExpirationMinutes"], out var min) ? min : 60;
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(accessTokenExpirationMinutes);

        return Result.Success(new AuthResultDto(
            user.Id,
            user.Email,
            accessToken,
            newRefreshTokenRaw,
            expiresAt));
    }
}
