using MediatR;
using Microsoft.Extensions.Logging;

namespace RianFriends.Application.Behaviours;

/// <summary>
/// MediatR 파이프라인 — 모든 Command/Query의 시작/완료/오류를 로깅합니다.
/// 성능 측정 및 디버깅을 위해 실행 시간을 함께 기록합니다.
/// </summary>
internal sealed class LoggingBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    /// <summary>생성자</summary>
    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var startTime = DateTime.UtcNow;

        _logger.LogInformation("Request 시작: {RequestName}", requestName);

        try
        {
            var response = await next();
            var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;

            _logger.LogInformation(
                "Request 완료: {RequestName} ({ElapsedMs}ms)",
                requestName,
                elapsed);

            return response;
        }
        catch (Exception ex)
        {
            var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;

            _logger.LogError(
                ex,
                "Request 실패: {RequestName} ({ElapsedMs}ms)",
                requestName,
                elapsed);

            throw;
        }
    }
}
