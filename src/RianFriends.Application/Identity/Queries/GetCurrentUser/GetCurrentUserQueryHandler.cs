using MediatR;
using RianFriends.Application.Identity.Dtos;
using RianFriends.Application.Identity.Interfaces;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Identity.Queries.GetCurrentUser;

/// <summary>현재 사용자 정보 Query 핸들러</summary>
public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    /// <summary>의존성을 주입합니다.</summary>
    public GetCurrentUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>현재 사용자 정보를 조회합니다.</summary>
    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserDto>("사용자를 찾을 수 없습니다.");
        }

        var dto = new UserDto(
            user.Id,
            user.Email,
            user.Nickname,
            user.BirthDate,
            user.CountryCode,
            user.Plan,
            user.Role,
            user.IsEmailHidden);

        return Result.Success(dto);
    }
}
