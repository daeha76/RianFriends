using MediatR;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Identity.Interfaces;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Identity.Commands.DeleteAccount;

/// <summary>계정 탈퇴 Command 핸들러. 개인정보를 즉시 삭제하고 Supabase Auth 계정을 제거합니다.</summary>
public class DeleteUserAccountCommandHandler : IRequestHandler<DeleteUserAccountCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;
    private readonly ILogger<DeleteUserAccountCommandHandler> _logger;

    /// <summary>의존성을 주입합니다.</summary>
    public DeleteUserAccountCommandHandler(
        IUserRepository userRepository,
        IAuthService authService,
        ILogger<DeleteUserAccountCommandHandler> logger)
    {
        _userRepository = userRepository;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>계정 탈퇴를 처리합니다.</summary>
    public async Task<Result> Handle(DeleteUserAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure("사용자를 찾을 수 없습니다.");
        }

        var deleteResult = user.Delete();
        if (deleteResult.IsFailure)
        {
            return deleteResult;
        }

        await _userRepository.SaveChangesAsync(cancellationToken);

        // Supabase Auth 계정 삭제
        await _authService.DeleteUserAsync(request.UserId, cancellationToken);

        _logger.LogInformation("사용자 계정 탈퇴 완료. UserId: {UserId}", request.UserId);
        return Result.Success();
    }
}
