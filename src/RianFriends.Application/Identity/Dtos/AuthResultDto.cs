namespace RianFriends.Application.Identity.Dtos;

/// <summary>인증 결과 DTO (MAUI SecureStorage에 저장용)</summary>
public record AuthResultDto(
    Guid UserId,
    string Email,
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt);
