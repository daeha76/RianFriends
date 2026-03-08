namespace RianFriends.Application.Identity.Dtos;

/// <summary>소셜 인증 후 획득한 사용자 정보 (JWT 발급 전 단계)</summary>
public record SocialUserInfo(
    Guid UserId,
    string Email,
    bool IsEmailHidden = false);
