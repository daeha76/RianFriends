using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RianFriends.Application.Notification.Commands.DeleteWakeUpAlarm;
using RianFriends.Application.Notification.Commands.RegisterDeviceToken;
using RianFriends.Application.Notification.Commands.SetWakeUpAlarm;
using RianFriends.Application.Notification.Queries.GetWakeUpAlarms;
using RianFriends.Domain.Notification;

namespace RianFriends.Api.Controllers;

/// <summary>기상 알람 및 디바이스 토큰 관리 API</summary>
[ApiController]
[Authorize]
[Produces("application/json")]
public class AlarmsController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>의존성을 주입합니다.</summary>
    public AlarmsController(ISender sender) => _sender = sender;

    /// <summary>내 기상 알람 목록 조회</summary>
    /// <param name="ct">취소 토큰</param>
    /// <response code="200">알람 목록 반환</response>
    [HttpGet("api/v1/alarms")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAlarms(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(new GetWakeUpAlarmsQuery(userId), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(detail: result.Error, statusCode: 500);
    }

    /// <summary>기상 알람 등록</summary>
    /// <param name="request">알람 설정 정보</param>
    /// <param name="ct">취소 토큰</param>
    /// <response code="201">알람 등록 성공, 알람 ID 반환</response>
    /// <response code="400">유효성 검증 실패</response>
    [HttpPost("api/v1/alarms")]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SetAlarm([FromBody] SetAlarmRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new SetWakeUpAlarmCommand(userId, request.FriendId, request.AlarmTime, request.RepeatDays), ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAlarms), new { }, result.Value)
            : Problem(detail: result.Error, statusCode: 400);
    }

    /// <summary>기상 알람 삭제</summary>
    /// <param name="alarmId">삭제할 알람 ID</param>
    /// <param name="ct">취소 토큰</param>
    /// <response code="204">삭제 성공</response>
    /// <response code="404">알람 없음</response>
    [HttpDelete("api/v1/alarms/{alarmId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteAlarm([FromRoute] Guid alarmId, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(new DeleteWakeUpAlarmCommand(userId, alarmId), ct);
        return result.IsSuccess ? NoContent() : Problem(detail: result.Error, statusCode: 404);
    }

    /// <summary>디바이스 토큰 등록 (푸시 알림용)</summary>
    /// <param name="request">토큰 정보</param>
    /// <param name="ct">취소 토큰</param>
    /// <response code="200">등록 성공</response>
    /// <response code="400">유효성 검증 실패</response>
    [HttpPost("api/v1/device-tokens")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RegisterDeviceToken([FromBody] RegisterTokenRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(new RegisterDeviceTokenCommand(userId, request.Token, request.Platform), ct);
        return result.IsSuccess ? Ok() : Problem(detail: result.Error, statusCode: 400);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst("sub") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim?.Value, out var id) ? id : Guid.Empty;
    }
}

/// <summary>알람 설정 요청 DTO</summary>
public record SetAlarmRequest(Guid FriendId, TimeOnly AlarmTime, byte RepeatDays = 0);

/// <summary>디바이스 토큰 등록 요청 DTO</summary>
public record RegisterTokenRequest(string Token, DevicePlatform Platform);
