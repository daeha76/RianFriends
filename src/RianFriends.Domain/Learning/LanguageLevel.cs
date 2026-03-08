namespace RianFriends.Domain.Learning;

/// <summary>
/// 사용자의 외국어 숙련도 레벨. 대화 분석으로 자동 측정 (명시적 테스트 없음).
/// 언어별로 독립 저장 (중국어 Middle, 영어 Elementary 가능).
/// </summary>
public enum LanguageLevel
{
    /// <summary>초급 이전: 단어 몇 개, 한국어 보조 최대</summary>
    Infant,

    /// <summary>초급: 짧은 문장, 칭찬/반복 위주</summary>
    Elementary,

    /// <summary>중급: 일상 문장, 자연스러운 교정</summary>
    Middle,

    /// <summary>고급: 관용어·슬랭 도입</summary>
    High,

    /// <summary>원어민 수준: 뉘앙스 중심</summary>
    Advanced
}
