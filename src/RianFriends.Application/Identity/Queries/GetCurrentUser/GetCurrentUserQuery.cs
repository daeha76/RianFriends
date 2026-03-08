using RianFriends.Application.Abstractions;
using RianFriends.Application.Identity.Dtos;

namespace RianFriends.Application.Identity.Queries.GetCurrentUser;

/// <summary>현재 인증된 사용자 정보 조회 Query</summary>
public record GetCurrentUserQuery(Guid UserId) : IQuery<UserDto>;
