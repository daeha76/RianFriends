using MediatR;
using RianFriends.Application.Abstractions;
using RianFriends.Application.Friend.Dtos;
using RianFriends.Domain.Common;

namespace RianFriends.Application.Friend.Queries.GetFriend;

/// <summary>AI 친구 목록 조회 핸들러</summary>
public sealed class GetFriendsQueryHandler : IRequestHandler<GetFriendsQuery, Result<List<FriendDto>>>
{
    private readonly IFriendRepository _friendRepository;

    /// <inheritdoc />
    public GetFriendsQueryHandler(IFriendRepository friendRepository)
    {
        _friendRepository = friendRepository;
    }

    /// <inheritdoc />
    public async Task<Result<List<FriendDto>>> Handle(GetFriendsQuery request, CancellationToken cancellationToken)
    {
        var friends = await _friendRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        var personas = new Dictionary<Guid, Domain.Friend.FriendPersona>();

        foreach (var friend in friends)
        {
            var persona = await _friendRepository.GetPersonaByIdAsync(friend.PersonaId, cancellationToken);
            if (persona is not null)
            {
                personas[friend.PersonaId] = persona;
            }
        }

        var dtos = friends.Select(f =>
        {
            personas.TryGetValue(f.PersonaId, out var persona);
            return new FriendDto(
                f.Id,
                f.PersonaId,
                persona?.Name ?? string.Empty,
                persona?.Nationality ?? string.Empty,
                persona?.TargetLanguage ?? string.Empty,
                persona?.Personality ?? Domain.Friend.PersonalityType.Energetic,
                persona?.Interests ?? [],
                persona?.SpeechStyle ?? Domain.Friend.SpeechStyle.Casual,
                f.IsActive);
        }).ToList();

        return Result.Success(dtos);
    }
}
