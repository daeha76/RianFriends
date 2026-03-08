using MediatR;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Identity.Dtos;
using RianFriends.Application.Identity.Interfaces;
using RianFriends.Domain.Common;
using RianFriends.Domain.Identity;

namespace RianFriends.Application.Identity.Commands.Login;

/// <summary>로그인 Command 핸들러. Supabase Auth 인증 후 사용자 정보를 DB에 동기화합니다.</summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResultDto>>
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<LoginCommandHandler> _logger;

    /// <summary>의존성을 주입합니다.</summary>
    public LoginCommandHandler(
        IAuthService authService,
        IUserRepository userRepository,
        ILogger<LoginCommandHandler> logger)
    {
        _authService = authService;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>로그인을 처리합니다. 최초 로그인 시 사용자 레코드를 DB에 생성합니다.</summary>
    public async Task<Result<AuthResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        AuthResultDto authResult;

        if (request.Provider == "email")
        {
            authResult = await _authService.SignInAsync(request.Email!, request.Credential, cancellationToken);
        }
        else
        {
            authResult = await _authService.SignInWithOAuthAsync(request.Provider, request.Credential, cancellationToken);
        }

        // 최초 소셜/이메일 로그인 시 DB에 사용자 레코드 자동 생성 (Upsert)
        var exists = await _userRepository.ExistsAsync(authResult.UserId, cancellationToken);
        if (!exists)
        {
            var createResult = User.Create(authResult.UserId, authResult.Email, request.IsEmailHidden);
            if (createResult.IsFailure)
            {
                return Result.Failure<AuthResultDto>(createResult.Error);
            }

            _userRepository.Add(createResult.Value);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("신규 사용자 DB 등록 완료. UserId: {UserId}", authResult.UserId);
        }

        _logger.LogInformation("사용자 로그인 성공. Provider: {Provider}, UserId: {UserId}", request.Provider, authResult.UserId);
        return Result.Success(authResult);
    }
}
