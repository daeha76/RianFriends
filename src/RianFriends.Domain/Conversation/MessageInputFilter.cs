namespace RianFriends.Domain.Conversation;

/// <summary>메시지 필터링 심각도 수준</summary>
public enum FilterSeverity
{
    /// <summary>안전한 메시지</summary>
    Safe,

    /// <summary>경고 (경미한 부적절 표현)</summary>
    Warned,

    /// <summary>차단 (폭력·음란 등 심각한 콘텐츠)</summary>
    Blocked,

    /// <summary>위기 (자살·자해 관련 키워드 — 즉시 차단 및 위기상담 안내)</summary>
    Crisis
}

/// <summary>메시지 필터 평가 결과</summary>
/// <param name="Severity">심각도 수준</param>
/// <param name="Reason">차단 이유 (Safe이면 null)</param>
/// <param name="CrisisMessage">위기 상황 안내 메시지 (Crisis이면 제공)</param>
public sealed record FilterResult(
    FilterSeverity Severity,
    string? Reason = null,
    string? CrisisMessage = null)
{
    /// <summary>안전 결과 기본 인스턴스</summary>
    public static readonly FilterResult Safe = new(FilterSeverity.Safe);
}

/// <summary>
/// 메시지 입력 필터 도메인 서비스.
/// 앱스토어 지침 및 사용자 안전을 위해 3단계 필터를 적용합니다.
/// </summary>
public static class MessageInputFilter
{
    private static readonly string[] CrisisKeywords =
    [
        "자살", "자해", "죽고 싶", "죽고싶", "목숨을 끊", "스스로 목숨",
        "suicide", "self-harm", "kill myself", "end my life", "hurt myself"
    ];

    private static readonly string[] BlockedKeywords =
    [
        "포르노", "음란", "야동", "성인물",
        "porn", "explicit sexual", "xxx",
        "폭탄 만드는", "테러", "살인 방법"
    ];

    private static readonly string[] WarnedKeywords =
    [
        "욕설", "병신", "씨발", "개새끼", "존나",
        "fuck", "shit", "asshole", "bitch"
    ];

    private const string CrisisGuidance =
        "지금 많이 힘드시죠. 혼자 감당하지 않아도 됩니다.\n" +
        "📞 자살예방상담전화: 1393 (24시간)\n" +
        "📞 정신건강 위기상담: 1577-0199";

    /// <summary>
    /// 메시지를 평가하여 <see cref="FilterResult"/>를 반환합니다.
    /// Crisis → Blocked → Warned → Safe 순으로 판정합니다.
    /// </summary>
    /// <param name="message">사용자가 입력한 메시지</param>
    public static FilterResult Evaluate(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return FilterResult.Safe;
        }

        var lower = message.ToLowerInvariant();

        foreach (var keyword in CrisisKeywords)
        {
            if (lower.Contains(keyword.ToLowerInvariant()))
            {
                return new FilterResult(FilterSeverity.Crisis,
                    Reason: "위기 상황 키워드가 감지되었습니다.",
                    CrisisMessage: CrisisGuidance);
            }
        }

        foreach (var keyword in BlockedKeywords)
        {
            if (lower.Contains(keyword.ToLowerInvariant()))
            {
                return new FilterResult(FilterSeverity.Blocked,
                    Reason: "부적절한 콘텐츠가 포함되어 전송이 차단되었습니다.");
            }
        }

        foreach (var keyword in WarnedKeywords)
        {
            if (lower.Contains(keyword.ToLowerInvariant()))
            {
                return new FilterResult(FilterSeverity.Warned,
                    Reason: "상대방에게 불편함을 줄 수 있는 표현이 포함되어 있습니다.");
            }
        }

        return FilterResult.Safe;
    }
}
