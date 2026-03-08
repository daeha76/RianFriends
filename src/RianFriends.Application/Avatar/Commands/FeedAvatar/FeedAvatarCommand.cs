using RianFriends.Application.Abstractions;

namespace RianFriends.Application.Avatar.Commands.FeedAvatar;

/// <summary>아바타에게 먹이를 주는 커맨드. 새 배고픔 레벨을 반환합니다.</summary>
public sealed record FeedAvatarCommand(Guid FriendId, string SnackType) : ICommand<int>;
