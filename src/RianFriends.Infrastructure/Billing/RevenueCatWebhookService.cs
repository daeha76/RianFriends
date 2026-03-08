using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Billing;
using RianFriends.Domain.Identity;

namespace RianFriends.Infrastructure.Billing;

/// <summary>RevenueCat Webhook 파싱 및 제품-플랜 매핑</summary>
internal sealed class RevenueCatWebhookService : IBillingService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>RevenueCat 제품 ID → PlanType 매핑 (앱스토어 제품 ID와 일치시켜야 함)</summary>
    private static readonly Dictionary<string, PlanType> ProductPlanMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "com.rianfriends.basic_monthly",  PlanType.Basic },
        { "com.rianfriends.basic_yearly",   PlanType.Basic },
        { "com.rianfriends.plus_monthly",   PlanType.Plus },
        { "com.rianfriends.plus_yearly",    PlanType.Plus },
        { "com.rianfriends.pro_monthly",    PlanType.Pro },
        { "com.rianfriends.pro_yearly",     PlanType.Pro }
    };

    private static readonly Dictionary<string, RevenueCatEventType> EventTypeMap =
        new(StringComparer.OrdinalIgnoreCase)
    {
        { "INITIAL_PURCHASE",  RevenueCatEventType.InitialPurchase },
        { "RENEWAL",           RevenueCatEventType.Renewal },
        { "PRODUCT_CHANGE",    RevenueCatEventType.ProductChange },
        { "CANCELLATION",      RevenueCatEventType.Cancellation },
        { "EXPIRATION",        RevenueCatEventType.Expiration },
        { "BILLING_ISSUE",     RevenueCatEventType.BillingIssue },
        { "SUBSCRIBER_ALIAS",  RevenueCatEventType.SubscriberAlias }
    };

    /// <inheritdoc />
    public RevenueCatWebhookPayload? ParseWebhook(string rawJson)
    {
        try
        {
            // RevenueCat Webhook은 { "event": { ... } } 구조로 전달됨
            using var doc = JsonDocument.Parse(rawJson);
            if (!doc.RootElement.TryGetProperty("event", out var eventElement))
            {
                return null;
            }

            var dto = eventElement.Deserialize<RevenueCatEventDto>(JsonOptions);
            if (dto is null)
            {
                return null;
            }

            return new RevenueCatWebhookPayload
            {
                EventType = dto.Type ?? string.Empty,
                AppUserId = dto.AppUserId ?? string.Empty,
                RevenueCatId = dto.Id ?? string.Empty,
                ProductId = dto.ProductId ?? string.Empty,
                ExpirationAtMs = dto.ExpirationAtMs
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public PlanType MapProductToPlan(string productId)
    {
        return ProductPlanMap.TryGetValue(productId, out var plan) ? plan : PlanType.Free;
    }

    /// <inheritdoc />
    public RevenueCatEventType? ParseEventType(string eventType)
    {
        return EventTypeMap.TryGetValue(eventType, out var type) ? type : null;
    }

    /// <summary>RevenueCat 이벤트 JSON DTO</summary>
    private sealed class RevenueCatEventDto
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("app_user_id")]
        public string? AppUserId { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("product_id")]
        public string? ProductId { get; set; }

        [JsonPropertyName("expiration_at_ms")]
        public long? ExpirationAtMs { get; set; }
    }
}
