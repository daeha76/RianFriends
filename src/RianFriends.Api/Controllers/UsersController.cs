using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RianFriends.Application.Identity.Commands.DeleteAccount;
using RianFriends.Application.Identity.Commands.RegisterUser;
using RianFriends.Application.Identity.Commands.UpdateProfile;
using RianFriends.Application.Identity.Interfaces;
using RianFriends.Application.Identity.Queries.GetCurrentUser;

namespace RianFriends.Api.Controllers;

/// <summary>사용자 API 컨트롤러</summary>
[ApiController]
[Route("api/v1/users")]
[Authorize]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentUserService _currentUser;

    /// <summary>의존성을 주입합니다.</summary>
    public UsersController(ISender sender, ICurrentUserService currentUser)
    {
        _sender = sender;
        _currentUser = currentUser;
    }

    /// <summary>현재 인증된 사용자 정보 조회</summary>
    /// <param name="ct">취소 토큰</param>
    /// <returns>사용자 정보 (Id, Email, Nickname, Plan, Role)</returns>
    /// <response code="200">조회 성공</response>
    /// <response code="401">인증 필요</response>
    /// <response code="404">사용자 없음</response>
    [HttpGet("me")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var result = await _sender.Send(new GetCurrentUserQuery(_currentUser.UserId), ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(Problem(detail: result.Error, statusCode: 404));
    }

    /// <summary>최초 프로필 등록 (소셜 로그인 후 닉네임/생년월일 설정)</summary>
    /// <param name="command">프로필 등록 요청 (Nickname, BirthDate, CountryCode)</param>
    /// <param name="ct">취소 토큰</param>
    /// <returns>등록된 사용자 정보</returns>
    /// <response code="200">등록 성공</response>
    /// <response code="400">유효성 검증 실패</response>
    /// <response code="401">인증 필요</response>
    [HttpPost("me/register")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(detail: result.Error, statusCode: 400);
    }

    /// <summary>프로필 수정 (닉네임/생년월일/국가 코드)</summary>
    /// <param name="command">수정 요청 (Nickname, BirthDate, CountryCode)</param>
    /// <param name="ct">취소 토큰</param>
    /// <returns>수정된 사용자 정보</returns>
    /// <response code="200">수정 성공</response>
    /// <response code="400">유효성 검증 실패</response>
    /// <response code="401">인증 필요</response>
    [HttpPut("me")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(detail: result.Error, statusCode: 400);
    }

    /// <summary>계정 탈퇴 (Soft Delete + 개인정보 즉시 삭제)</summary>
    /// <param name="ct">취소 토큰</param>
    /// <returns>탈퇴 완료</returns>
    /// <response code="204">탈퇴 성공</response>
    /// <response code="400">탈퇴 처리 실패</response>
    /// <response code="401">인증 필요</response>
    [HttpDelete("me")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> DeleteMe(CancellationToken ct)
    {
        var result = await _sender.Send(new DeleteUserAccountCommand(_currentUser.UserId), ct);
        return result.IsSuccess
            ? NoContent()
            : Problem(detail: result.Error, statusCode: 400);
    }
}

