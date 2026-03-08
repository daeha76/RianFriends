using MediatR;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Abstractions;
using RianFriends.Application.Identity.Interfaces;
using RianFriends.Domain.Billing;
using RianFriends.Domain.Common;
using RianFriends.Domain.Identity;

namespace RianFriends.Application.Billing.Commands.HandleRevenueCatWebhook;

/// <summary>
/// RevenueCat Webhook 처리 핸들러.
/// 이벤트 유형에 따라 사용자 플랜을 업데이트하고 구독 이력을 관리합니다.
/// </summary>
public sealed class HandleRevenueCatWebhookCommandHandler
    : IRequestHandler<HandleRevenueCatWebhookCommand, Result>
{
    private readonly IBillingService _billingService;
    private readonly IUserRepository _userRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUserQuotaRepository _userQuotaRepository;
    private readonly ILogger<HandleRevenueCatWebhookCommandHandler> _logger;

    /// <inheritdoc />
    public HandleRevenueCatWebhookCommandHandler(
        IBillingService billingService,
        IUserRepository userRepository,
        ISubscriptionRepository subscriptionRepository,
        IUserQuotaRepository userQuotaRepository,
        ILogger<HandleRevenueCatWebhookCommandHandler> logger)
    {
        _billingService = billingService;
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
        _userQuotaRepository = userQuotaRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(
        HandleRevenueCatWebhookCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Payload 파싱
        var payload = _billingService.ParseWebhook(request.RawJson);
        if (payload is null)
        {
            _logger.LogWarning("RevenueCat Webhook 파싱 실패");
            return Result.Failure("Webhook 페이로드를 파싱할 수 없습니다.");
        }

        // 2. 이벤트 유형 파싱
        var eventType = _billingService.ParseEventType(payload.EventType);
        if (eventType is null)
        {
            _logger.LogInformation("알 수 없는 RevenueCat 이벤트 유형: {EventType}", payload.EventType);
            return Result.Success(); // 알 수 없는 이벤트는 무시
        }

        // 3. 사용자 조회 (AppUserId = Supabase User Id)
        if (!Guid.TryParse(payload.AppUserId, out var userId))
        {
            return Result.Failure($"유효하지 않은 AppUserId: {payload.AppUserId}");
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("RevenueCat Webhook: 사용자를 찾을 수 없음. UserId={UserId}", userId);
            return Result.Failure("사용자를 찾을 수 없습니다.");
        }

        _logger.LogInformation(
            "RevenueCat Webhook 처리: EventType={EventType}, UserId={UserId}, ProductId={ProductId}",
            payload.EventType, userId, payload.ProductId);

        // 4. 이벤트 유형별 처리
        switch (eventType)
        {
            case RevenueCatEventType.InitialPurchase:
            case RevenueCatEventType.Renewal:
            case RevenueCatEventType.ProductChange:
                return await HandleActivationAsync(user, payload, cancellationToken);

            case RevenueCatEventType.Cancellation:
            case RevenueCatEventType.Expiration:
            case RevenueCatEventType.BillingIssue:
                return await HandleDeactivationAsync(user, cancellationToken);

            default:
                return Result.Success(); // SubscriberAlias 등 무시
        }
    }

    private async Task<Result> HandleActivationAsync(
        Domain.Identity.User user,
        RevenueCatWebhookPayload payload,
        CancellationToken cancellationToken)
    {
        var newPlan = _billingService.MapProductToPlan(payload.ProductId);
        var expiresAt = payload.ExpirationAtMs.HasValue
            ? DateTimeOffset.FromUnixTimeMilliseconds(payload.ExpirationAtMs.Value)
            : DateTimeOffset.UtcNow.AddMonths(1);

        // 기존 활성 구독 비활성화
        var existingSubscription = await _subscriptionRepository.GetActiveByUserIdAsync(user.Id, cancellationToken);
        if (existingSubscription is not null)
        {
            existingSubscription.Deactivate();
        }

        // 새 구독 생성
        var subscriptionResult = Subscription.Create(
            user.Id, payload.RevenueCatId, payload.ProductId, newPlan, expiresAt);

        if (subscriptionResult.IsFailure)
        {
            return Result.Failure(subscriptionResult.Error);
        }

        _subscriptionRepository.Add(subscriptionResult.Value);

        // 사용자 플랜 업데이트
        user.UpdatePlan(newPlan);

        // 오늘 쿼터 한도 업데이트
        var quota = await _userQuotaRepository.GetTodayAsync(user.Id, cancellationToken);
        quota?.UpdateLimit(newPlan);

        await _userRepository.SaveChangesAsync(cancellationToken);
        await _subscriptionRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result> HandleDeactivationAsync(
        Domain.Identity.User user,
        CancellationToken cancellationToken)
    {
        var existingSubscription = await _subscriptionRepository.GetActiveByUserIdAsync(user.Id, cancellationToken);
        if (existingSubscription is not null)
        {
            existingSubscription.Deactivate();
            await _subscriptionRepository.SaveChangesAsync(cancellationToken);
        }

        // Free 플랜으로 다운그레이드
        user.UpdatePlan(PlanType.Free);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
