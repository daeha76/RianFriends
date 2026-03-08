using RianFriends.Application.Abstractions;
using RianFriends.Application.Identity.Dtos;

namespace RianFriends.Application.Identity.Commands.RefreshToken;

/// <summary>AccessToken 갱신 Command</summary>
public record RefreshTokenCommand(string RefreshToken) : ICommand<AuthResultDto>;
