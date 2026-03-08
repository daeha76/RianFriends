using MediatR;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Identity.Dtos;
using RianFriends.Application.Identity.Interfaces;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Identity.Commands.UpdateProfile;

/// <summary>사용자 프로필 수정 핸들러.</summary>
public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpdateProfileCommandHandler> _logger;

    /// <summary>의존성을 주입합니다.</summary>
    public UpdateProfileCommandHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUser,
        ILogger<UpdateProfileCommandHandler> logger)
    {
        _userRepository = userRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>사용자 프로필을 수정합니다.</summary>
    public async Task<Result<UserDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(_currentUser.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserDto>("사용자를 찾을 수 없습니다.");
        }

        var updateResult = user.UpdateProfile(request.Nickname, request.BirthDate, request.CountryCode);
        if (updateResult.IsFailure)
        {
            return Result.Failure<UserDto>(updateResult.Error);
        }

        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("사용자 프로필 수정 완료. UserId: {UserId}", user.Id);

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
