using RianFriends.Domain.Identity;

namespace RianFriends.Application.Identity.Dtos;

/// <summary>사용자 정보 DTO</summary>
public record UserDto(
    Guid Id,
    string Email,
    string? Nickname,
    DateOnly? BirthDate,
    string? CountryCode,
    PlanType Plan,
    UserRole Role,
    bool IsEmailHidden);
