using RianFriends.Application.Abstractions;
using RianFriends.Application.Identity.Dtos;

namespace RianFriends.Application.Identity.Commands.UpdateProfile;

/// <summary>사용자 프로필 수정 Command.</summary>
public record UpdateProfileCommand(
    string Nickname,
    DateOnly BirthDate,
    string CountryCode) : ICommand<UserDto>;
