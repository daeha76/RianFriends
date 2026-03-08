using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RianFriends.Application.Conversation.Commands.EndConversation;
using RianFriends.Application.Conversation.Commands.SendMessage;
using RianFriends.Application.Conversation.Commands.SetEmpathyGauge;
using RianFriends.Application.Conversation.Commands.StartConversation;
using RianFriends.Domain.Conversation;

namespace RianFriends.Api.Controllers;

/// <summary>AI 친구 대화 API</summary>
[ApiController]
[Route("api/v1/conversations")]
[Authorize]
[Produces("application/json")]
public class ConversationsController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>의존성을 주입합니다.</summary>
    public ConversationsController(ISender sender) => _sender = sender;

    /// <summary>새 대화 세션 시작</summary>
    /// <param name="request">친구 ID</param>
    /// <param name="ct">취소 토큰</param>
    /// <response code="201">세션 생성 성공, SessionId 반환</response>
    /// <response code="400">유효하지 않은 친구</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> StartConversation([FromBody] StartConversationRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(new StartConversationCommand(userId, request.FriendId), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(StartConversation), new { }, result.Value)
            : Problem(detail: result.Error, statusCode: 400);
    }

    /// <summary>
    /// 메시지 전송 (SSE 스트리밍).
    /// 응답은 text/event-stream 형식으로 토큰 단위로 스트리밍됩니다.
    /// </summary>
    /// <param name="sessionId">대화 세션 ID</param>
    /// <param name="request">메시지 내용</param>
    /// <param name="ct">취소 토큰</param>
    /// <response code="200">SSE 스트리밍 응답</response>
    /// <response code="400">유효하지 않은 세션 또는 메시지</response>
    [HttpPost("{sessionId:guid}/messages")]
    [EnableRateLimiting("ConversationPolicy")]
    [Produces("text/event-stream")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task SendMessage(Guid sessionId, [FromBody] SendMessageRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var result = await _sender.Send(new SendMessageCommand(sessionId, userId, request.Content), ct);
        if (result.IsFailure)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            await Response.WriteAsync(result.Error, ct);
            return;
        }

        // SSE 스트리밍 응답
        Response.Headers.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";

        var fullResponse = new StringBuilder();
        await foreach (var chunk in result.Value.WithCancellation(ct))
        {
            fullResponse.Append(chunk);
            var sseData = $"data: {chunk}\n\n";
            await Response.WriteAsync(sseData, ct);
            await Response.Body.FlushAsync(ct);
        }

        await Response.WriteAsync("data: [DONE]\n\n", ct);
        await Response.Body.FlushAsync(ct);
    }

    /// <summary>공감 게이지 설정</summary>
    /// <param name="sessionId">세션 ID</param>
    /// <param name="request">게이지 값 (0–100) 및 제어 방식</param>
    /// <param name="ct">취소 토큰</param>
    /// <response code="200">설정 성공</response>
    /// <response code="400">유효하지 않은 게이지 값</response>
    [HttpPut("{sessionId:guid}/empathy")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SetEmpathyGauge(Guid sessionId, [FromBody] SetEmpathyGaugeRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(new SetEmpathyGaugeCommand(sessionId, userId, request.Gauge, request.ControlMode), ct);
        return result.IsSuccess ? Ok() : Problem(detail: result.Error, statusCode: 400);
    }

    /// <summary>대화 세션 종료</summary>
    /// <param name="sessionId">세션 ID</param>
    /// <param name="ct">취소 토큰</param>
    /// <response code="204">종료 성공</response>
    /// <response code="400">유효하지 않은 세션</response>
    [HttpDelete("{sessionId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> EndConversation(Guid sessionId, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(new EndConversationCommand(sessionId, userId), ct);
        return result.IsSuccess ? NoContent() : Problem(detail: result.Error, statusCode: 400);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst("sub") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim?.Value, out var id) ? id : Guid.Empty;
    }
}

/// <summary>대화 세션 시작 요청 DTO</summary>
public record StartConversationRequest(Guid FriendId);

/// <summary>메시지 전송 요청 DTO</summary>
public record SendMessageRequest(string Content);

/// <summary>공감 게이지 설정 요청 DTO</summary>
public record SetEmpathyGaugeRequest(int Gauge, GaugeControlMode ControlMode);
