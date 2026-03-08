using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using RianFriends.Application.Billing.Commands.HandleRevenueCatWebhook;
using System.Security.Cryptography;
using System.Text;

namespace RianFriends.Api.Controllers;

/// <summary>Billing (RevenueCat Webhook) API 컨트롤러</summary>
[ApiController]
[Route("api/v1/webhooks")]
public class BillingController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<BillingController> _logger;
    private readonly IConfiguration _configuration;

    /// <inheritdoc />
    public BillingController(ISender sender, ILogger<BillingController> logger, IConfiguration configuration)
    {
        _sender = sender;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>RevenueCat Webhook 수신 엔드포인트</summary>
    /// <remarks>
    /// RevenueCat 대시보드에서 설정한 Shared Secret으로 서명을 검증합니다.
    /// 검증에 실패하면 401을 반환합니다.
    /// </remarks>
    [HttpPost("revenuecat")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleRevenueCatWebhook(CancellationToken cancellationToken)
    {
        // Request body를 Raw JSON으로 읽기
        string rawJson;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            rawJson = await reader.ReadToEndAsync(cancellationToken);
        }

        // RevenueCat-Signature 헤더 검증 (fail-closed: secret 미설정 시 거부)
        var sharedSecret = _configuration["RevenueCat:WebhookSharedSecret"];
        if (string.IsNullOrEmpty(sharedSecret))
        {
            _logger.LogError("RevenueCat:WebhookSharedSecret이 설정되지 않았습니다. Webhook 수신 거부.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Webhook secret not configured");
        }

        var signature = Request.Headers["RevenueCat-Signature"].FirstOrDefault();
        if (string.IsNullOrEmpty(signature) || !ValidateSignature(rawJson, signature, sharedSecret))
        {
            _logger.LogWarning("RevenueCat Webhook 서명 검증 실패");
            return Unauthorized("Invalid webhook signature");
        }

        var command = new HandleRevenueCatWebhookCommand(rawJson, signature);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogError("RevenueCat Webhook 처리 실패: {Error}", result.Error);
            // RevenueCat은 2xx 외의 응답을 받으면 재시도하므로 200으로 응답
            // (이미 처리된 이벤트 중복 방지)
            return Ok();
        }

        return Ok();
    }

    /// <summary>RevenueCat HMAC-SHA256 서명을 검증합니다.</summary>
    private static bool ValidateSignature(string payload, string signature, string secret)
    {
        try
        {
            var secretBytes = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA256(secretBytes);
            var computedHash = hmac.ComputeHash(payloadBytes);
            var computedSignature = Convert.ToHexString(computedHash).ToLowerInvariant();

            // Timing-safe 비교 (타이밍 공격 방어)
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedSignature),
                Encoding.UTF8.GetBytes(signature.ToLowerInvariant()));
        }
        catch
        {
            return false;
        }
    }
}
