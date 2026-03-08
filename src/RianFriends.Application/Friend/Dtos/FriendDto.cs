using RianFriends.Domain.Friend;

namespace RianFriends.Application.Friend.Dtos;

/// <summary>AI 친구 정보 DTO</summary>
public record FriendDto(
    Guid Id,
    Guid PersonaId,
    string PersonaName,
    string Nationality,
    string TargetLanguage,
    PersonalityType Personality,
    string[] Interests,
    SpeechStyle SpeechStyle,
    bool IsActive);
