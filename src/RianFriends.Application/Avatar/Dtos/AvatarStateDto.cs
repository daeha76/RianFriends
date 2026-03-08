namespace RianFriends.Application.Avatar.Dtos;

/// <summary>아바타 상태 DTO</summary>
public sealed record AvatarStateDto(
    Guid FriendId,
    int HungerLevel,
    string HungerStatus,
    DateTimeOffset LastFedAt);
