using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Memory;

namespace RianFriends.Infrastructure.BackgroundJobs;

/// <summary>
/// 계층적 메모리 요약 배치 잡.
/// 만료된 ShortTerm → MidTerm 메모리를 LLM으로 요약하여 상위 레이어에 저장합니다.
/// 절대로 실시간 대화 흐름에 삽입하지 않습니다 — 배치 처리 전용.
/// </summary>
public sealed class MemorySummaryJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MemorySummaryJob> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

    /// <inheritdoc />
    public MemorySummaryJob(IServiceScopeFactory scopeFactory, ILogger<MemorySummaryJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MemorySummaryJob 시작");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(CheckInterval, stoppingToken);

            try
            {
                await ProcessExpiredMemoriesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "MemorySummaryJob 실행 중 오류 발생");
            }
        }
    }

    private async Task ProcessExpiredMemoriesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var memoryRepository = scope.ServiceProvider.GetRequiredService<IMemoryRepository>();
        var llmService = scope.ServiceProvider.GetRequiredService<ILlmService>();
        var conversationRepository = scope.ServiceProvider.GetRequiredService<IConversationRepository>();

        // ShortTerm 만료 → MidTerm 요약 생성
        var expiredShortTerm = await memoryRepository.GetExpiredMemoriesAsync(MemoryLayer.ShortTerm, ct);
        _logger.LogInformation("만료된 ShortTerm 메모리: {Count}건", expiredShortTerm.Count);

        foreach (var expired in expiredShortTerm)
        {
            try
            {
                // 해당 친구의 최근 메시지를 가져와서 요약
                var recentMessages = await conversationRepository.GetRecentMessagesByFriendIdAsync(
                    expired.FriendId,
                    50, ct);

                var summary = await llmService.SummarizeMemoryAsync(
                    [.. recentMessages],
                    MemoryLayer.MidTerm,
                    ct);

                var newMemory = FriendMemory.Create(expired.FriendId, MemoryLayer.MidTerm, summary);
                if (newMemory.IsSuccess)
                {
                    memoryRepository.Add(newMemory.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메모리 요약 실패: MemoryId={MemoryId}", expired.Id);
            }
        }

        await memoryRepository.SaveChangesAsync(ct);
        _logger.LogInformation("MemorySummaryJob 사이클 완료");
    }
}
