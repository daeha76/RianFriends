using RianFriends.Application.Abstractions;
using RianFriends.Domain.Notification;

namespace RianFriends.Application.Notification.Commands.RegisterDeviceToken;

/// <summary>디바이스 토큰 등록 커맨드</summary>
public sealed record RegisterDeviceTokenCommand(
    Guid UserId,
    string Token,
    DevicePlatform Platform) : ICommand;
