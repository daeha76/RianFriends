using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RianFriends.Application.Friend.Commands.CreateFriend;
using RianFriends.Application.Friend.Queries.GetFriend;

namespace RianFriends.Api.Controllers;

/// <summary>AI 친구 관리 API</summary>
[ApiController]
[Route("api/v1/friends")]
[Authorize]
[Produces("application/json")]
public class FriendsController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>의존성을 주입합니다.</summary>
    public FriendsController(ISender sender) => _sender = sender;

    /// <summary>내 AI 친구 목록 조회</summary>
    /// <response code="200">친구 목록 반환</response>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetFriends(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(new GetFriendsQuery(userId), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(detail: result.Error, statusCode: 500);
    }

    /// <summary>AI 친구 생성</summary>
    /// <param name="request">페르소나 ID</param>
    /// <param name="ct">취소 토큰</param>
    /// <response code="201">친구 생성 성공, 친구 ID 반환</response>
    /// <response code="400">유효성 검증 실패 또는 플랜 제한 초과</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateFriend([FromBody] CreateFriendRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(new CreateFriendCommand(userId, request.PersonaId), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetFriends), new { }, result.Value)
            : Problem(detail: result.Error, statusCode: 400);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst("sub") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim?.Value, out var id) ? id : Guid.Empty;
    }
}

/// <summary>친구 생성 요청 DTO</summary>
public record CreateFriendRequest(Guid PersonaId);
