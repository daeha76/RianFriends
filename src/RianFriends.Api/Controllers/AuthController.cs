using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RianFriends.Application.Identity.Commands.Login;
using RianFriends.Application.Identity.Commands.RefreshToken;

namespace RianFriends.Api.Controllers;

/// <summary>인증 API 컨트롤러</summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>의존성을 주입합니다.</summary>
    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>소셜/이메일 로그인</summary>
    /// <param name="command">로그인 요청 (Provider, Credential, Email)</param>
    /// <param name="ct">취소 토큰</param>
    /// <returns>AccessToken, RefreshToken, ExpiresAt</returns>
    /// <response code="200">로그인 성공</response>
    /// <response code="400">유효성 검증 실패</response>
    /// <response code="401">인증 실패</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(detail: result.Error, statusCode: 401);
    }

    /// <summary>AccessToken 갱신</summary>
    /// <param name="command">RefreshToken</param>
    /// <param name="ct">취소 토큰</param>
    /// <returns>새 AccessToken, RefreshToken, ExpiresAt</returns>
    /// <response code="200">갱신 성공</response>
    /// <response code="400">유효성 검증 실패</response>
    /// <response code="401">갱신 실패</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(detail: result.Error, statusCode: 401);
    }
}
