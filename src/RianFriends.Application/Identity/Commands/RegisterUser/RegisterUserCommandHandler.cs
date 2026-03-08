using MediatR;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Identity.Dtos;
using RianFriends.Application.Identity.Interfaces;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Identity.Commands.RegisterUser;

/// <summary>신규 사용자 프로필 등록 핸들러. 로그인 후 최초 닉네임/생년월일 설정 시 호출합니다.</summary>
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    /// <summary>의존성을 주입합니다.</summary>
    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUser,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>사용자 프로필을 등록합니다.</summary>
    public async Task<Result<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(_currentUser.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserDto>("사용자를 찾을 수 없습니다. 먼저 로그인하세요.");
        }

        var updateResult = user.UpdateProfile(request.Nickname, request.BirthDate, request.CountryCode);
        if (updateResult.IsFailure)
        {
            return Result.Failure<UserDto>(updateResult.Error);
        }

        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("사용자 프로필 등록 완료. UserId: {UserId}", user.Id);

        return Result.Success(new UserDto(
            user.Id,
            user.Email,
            user.Nickname,
            user.BirthDate,
            user.CountryCode,
            user.Plan,
            user.Role,
            user.IsEmailHidden));
    }
}
