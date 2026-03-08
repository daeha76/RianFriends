using RianFriends.Application.Abstractions;
using RianFriends.Application.Avatar.Dtos;

namespace RianFriends.Application.Avatar.Queries.GetAvatarState;

/// <summary>아바타 상태 조회 쿼리. UserId로 소유권을 검증합니다.</summary>
public sealed record GetAvatarStateQuery(Guid UserId, Guid FriendId) : IQuery<AvatarStateDto>;
