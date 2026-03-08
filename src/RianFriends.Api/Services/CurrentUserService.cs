using System.Security.Claims;
using RianFriends.Application.Identity.Interfaces;

namespace RianFriends.Api.Services;

/// <summary>JWT 클레임에서 현재 인증된 사용자 컨텍스트를 제공합니다.</summary>
internal sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>의존성을 주입합니다.</summary>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Guid UserId
    {
        get
        {
            var sub = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("sub");

            if (Guid.TryParse(sub, out var userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("인증된 사용자 ID를 확인할 수 없습니다.");
        }
    }

    /// <inheritdoc />
    public string Role
        => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role) ?? "user";

    /// <inheritdoc />
    public bool IsAdmin => Role == "admin";
}
