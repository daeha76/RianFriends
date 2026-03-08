using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RianFriends.Application.Avatar.Commands.FeedAvatar;
using RianFriends.Application.Avatar.Queries.GetAvatarState;

namespace RianFriends.Api.Controllers;

/// <summary>아바타 배고픔 관리 API</summary>
[ApiController]
[Route("api/v1/avatars")]
[Authorize]
[Produces("application/json")]
public class AvatarsController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>의존성을 주입합니다.</summary>
    public AvatarsController(ISender sender) => _sender = sender;

    /// <summary>아바타 현재 상태 조회</summary>
    /// <param name="friendId">AI 친구 ID</param>
    /// <param name="ct">취소 토큰</param>
    /// <response code="200">아바타 상태 반환</response>
    /// <response code="400">잘못된 요청</response>
    [HttpGet("{friendId:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetAvatarState([FromRoute] Guid friendId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetAvatarStateQuery(friendId), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(detail: result.Error, statusCode: 400);
    }

    /// <summary>아바타에게 먹이주기</summary>
    /// <param name="friendId">AI 친구 ID</param>
    /// <param name="request">간식 종류</param>
    /// <param name="ct">취소 토큰</param>
    /// <response code="200">새 배고픔 레벨 반환</response>
    /// <response code="400">유효성 검증 실패</response>
    [HttpPost("{friendId:guid}/feed")]
    [ProducesResponseType(typeof(int), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> FeedAvatar(
        [FromRoute] Guid friendId,
        [FromBody] FeedAvatarRequest request,
        CancellationToken ct)
    {
        var result = await _sender.Send(new FeedAvatarCommand(friendId, request.SnackType), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(detail: result.Error, statusCode: 400);
    }
}

/// <summary>먹이주기 요청 DTO</summary>
public record FeedAvatarRequest(string SnackType);
