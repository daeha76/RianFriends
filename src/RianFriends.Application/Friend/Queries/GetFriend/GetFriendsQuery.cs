using MediatR;
using RianFriends.Application.Friend.Dtos;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Friend.Queries.GetFriend;

/// <summary>내 AI 친구 목록 조회 쿼리</summary>
public record GetFriendsQuery(Guid UserId) : IRequest<Result<List<FriendDto>>>;
