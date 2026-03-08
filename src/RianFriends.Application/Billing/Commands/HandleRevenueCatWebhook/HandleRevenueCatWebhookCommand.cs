using RianFriends.Application.Abstractions;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Billing.Commands.HandleRevenueCatWebhook;

/// <summary>RevenueCat Webhook 처리 커맨드</summary>
/// <param name="RawJson">Webhook 원시 JSON 페이로드</param>
/// <param name="Signature">RevenueCat-Signature 헤더 값 (null이면 검증 생략)</param>
public sealed record HandleRevenueCatWebhookCommand(string RawJson, string? Signature)
    : ICommand;
