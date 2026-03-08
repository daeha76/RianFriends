using RianFriends.Domain.Common;

namespace RianFriends.Domain.Friend;

/// <summary>
/// AI 친구의 페르소나(캐릭터 정체성). 모든 대화 응답 시스템 프롬프트에 반드시 포함됩니다.
/// </summary>
public sealed class FriendPersona : AuditableEntity
{
    /// <summary>페르소나 이름 (예: "Mei", "Yuki", "Emma")</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>국적 코드 (예: "zh-CN", "ja", "en-GB")</summary>
    public string Nationality { get; private set; } = string.Empty;

    /// <summary>사용자가 배우는 언어 코드 (예: "zh-CN", "en", "ja")</summary>
    public string TargetLanguage { get; private set; } = string.Empty;

    /// <summary>성격 유형</summary>
    public PersonalityType Personality { get; private set; }

    /// <summary>관심사 목록 (예: ["kpop", "cooking", "gaming"])</summary>
    public string[] Interests { get; private set; } = [];

    /// <summary>말투 스타일</summary>
    public SpeechStyle SpeechStyle { get; private set; }

    /// <summary>EF Core용 기본 생성자</summary>
    private FriendPersona() { }

    /// <summary>새 페르소나를 생성합니다.</summary>
    public static Result<FriendPersona> Create(
        string name,
        string nationality,
        string targetLanguage,
        PersonalityType personality,
        string[] interests,
        SpeechStyle speechStyle)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<FriendPersona>("페르소나 이름은 필수입니다.");
        }

        if (string.IsNullOrWhiteSpace(nationality))
        {
            return Result.Failure<FriendPersona>("국적 코드는 필수입니다.");
        }

        if (string.IsNullOrWhiteSpace(targetLanguage))
        {
            return Result.Failure<FriendPersona>("학습 대상 언어는 필수입니다.");
        }

        var persona = new FriendPersona
        {
            Name = name,
            Nationality = nationality,
            TargetLanguage = targetLanguage,
            Personality = personality,
            Interests = interests,
            SpeechStyle = speechStyle
        };

        return Result.Success(persona);
    }

    /// <summary>
    /// 이 페르소나를 기반으로 LLM 시스템 프롬프트 기본 섹션을 생성합니다.
    /// </summary>
    public string BuildSystemPromptSection()
    {
        var interestsList = Interests.Length > 0
            ? string.Join(", ", Interests)
            : "다양한 주제";

        return $"""
            [AI Friend Persona]
            Name: {Name}
            Nationality: {Nationality}
            Target Language: {TargetLanguage}
            Personality: {Personality}
            Interests: {interestsList}
            Speech Style: {SpeechStyle}
            """;
    }
}
