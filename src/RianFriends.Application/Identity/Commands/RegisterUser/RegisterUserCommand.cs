using RianFriends.Application.Abstractions;
using RianFriends.Application.Identity.Dtos;

namespace RianFriends.Application.Identity.Commands.RegisterUser;

/// <summary>
/// 신규 사용자 프로필 등록 Command.
/// 소셜 로그인 후 닉네임/생년월일/국가 코드를 최초 설정할 때 호출합니다.
/// </summary>
public record RegisterUserCommand(
    string Nickname,
    DateOnly BirthDate,
    string CountryCode) : ICommand<UserDto>;
