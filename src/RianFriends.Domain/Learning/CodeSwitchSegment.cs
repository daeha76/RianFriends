namespace RianFriends.Domain.Learning;

/// <summary>
/// Code-Switching 파싱 결과 항목.
/// 사용자의 혼용 언어를 즉시 파싱하여 구조화된 데이터로 변환합니다.
/// 파싱은 메시지 저장 후 비동기로 처리됩니다 (응답 지연 방지).
/// </summary>
/// <param name="Original">원문 텍스트 (예: "你好")</param>
/// <param name="Romanized">병음/로마자 발음 표기 (예: "nǐ hǎo")</param>
/// <param name="Meaning">한국어 의미 (예: "안녕하세요")</param>
/// <param name="Language">언어 코드 (예: "zh-CN", "en", "ja")</param>
public record CodeSwitchSegment(
    string Original,
    string Romanized,
    string Meaning,
    string Language);
