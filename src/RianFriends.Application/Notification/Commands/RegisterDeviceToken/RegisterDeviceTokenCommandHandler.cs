using MediatR;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Common;
using RianFriends.Domain.Notification;

namespace RianFriends.Application.Notification.Commands.RegisterDeviceToken;

/// <summary>디바이스 토큰 등록 핸들러</summary>
public sealed class RegisterDeviceTokenCommandHandler : IRequestHandler<RegisterDeviceTokenCommand, Result>
{
    private readonly IDeviceTokenRepository _deviceTokenRepository;
    private readonly ILogger<RegisterDeviceTokenCommandHandler> _logger;

    /// <inheritdoc />
    public RegisterDeviceTokenCommandHandler(
        IDeviceTokenRepository deviceTokenRepository,
        ILogger<RegisterDeviceTokenCommandHandler> logger)
    {
        _deviceTokenRepository = deviceTokenRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(RegisterDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenResult = DeviceToken.Create(request.UserId, request.Token, request.Platform);

        if (tokenResult.IsFailure)
        {
            return Result.Failure(tokenResult.Error);
        }

        _deviceTokenRepository.Add(tokenResult.Value);
        await _deviceTokenRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "디바이스 토큰 등록: UserId={UserId}, Platform={Platform}",
            request.UserId,
            request.Platform);

        return Result.Success();
    }
}
