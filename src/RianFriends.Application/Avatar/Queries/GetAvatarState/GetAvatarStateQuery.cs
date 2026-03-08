using RianFriends.Application.Abstractions;
using RianFriends.Application.Avatar.Dtos;

namespace RianFriends.Application.Avatar.Queries.GetAvatarState;

/// <summary>아바타 상태 조회 쿼리</summary>
public sealed record GetAvatarStateQuery(Guid FriendId) : IQuery<AvatarStateDto>;
