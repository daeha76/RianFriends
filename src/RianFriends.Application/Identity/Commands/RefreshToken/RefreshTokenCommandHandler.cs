using MediatR;
using RianFriends.Application.Identity.Dtos;
using RianFriends.Application.Identity.Interfaces;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Identity.Commands.RefreshToken;

/// <summary>AccessToken 갱신 Command 핸들러</summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResultDto>>
{
    private readonly IAuthService _authService;

    /// <summary>의존성을 주입합니다.</summary>
    public RefreshTokenCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>RefreshToken으로 AccessToken을 갱신합니다.</summary>
    public async Task<Result<AuthResultDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshAsync(request.RefreshToken, cancellationToken);
        return Result.Success(result);
    }
}
