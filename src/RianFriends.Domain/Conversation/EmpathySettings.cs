using RianFriends.Domain.Common;
using RianFriends.Domain.Exceptions;

namespace RianFriends.Domain.Conversation;

/// <summary>
/// 공감 게이지 Value Object. 대화 세션 내 공감 응답 비율을 결정합니다.
/// 친구 성격별 기본값: Quiet=70, Energetic=60, Playful=50, Serious=30.
/// </summary>
public sealed class EmpathySettings
{
    /// <summary>공감 게이지 (0–100). 값이 높을수록 감성 공감 응답 비율 증가.</summary>
    public int Gauge { get; private set; }

    /// <summary>현재 대화 모드</summary>
    public ConversationMode Mode { get; private set; }

    /// <summary>게이지 제어 방식 (자동 감지 vs 수동 설정)</summary>
    public GaugeControlMode ControlMode { get; private set; }

    /// <summary>기본 생성자 (초기 상태: Language 모드, Auto 제어)</summary>
    public EmpathySettings()
    {
        Mode = ConversationMode.Language;
        ControlMode = GaugeControlMode.Auto;
    }

    /// <summary>공감 게이지와 모드를 설정합니다.</summary>
    /// <param name="gauge">게이지 값 (0–100)</param>
    /// <param name="source">설정 주체 (Auto 또는 ManualOverride)</param>
    public void SetGauge(int gauge, GaugeControlMode source)
    {
        if (gauge < 0 || gauge > 100)
        {
            throw new DomainException("게이지는 0–100 범위여야 합니다.");
        }

        Gauge = gauge;
        ControlMode = source;
        Mode = gauge > 0 ? ConversationMode.Empathy : ConversationMode.Language;
    }

    /// <summary>친구의 성격 기본 게이지로 초기화합니다 (세션 종료 시 호출).</summary>
    public void ResetToPersonalityDefault(int defaultGauge)
    {
        Gauge = defaultGauge;
        ControlMode = GaugeControlMode.Auto;
        Mode = ConversationMode.Language;
    }

    /// <summary>
    /// 현재 게이지에 기반하여 LLM 시스템 프롬프트에 삽입할 공감 지시 섹션을 생성합니다.
    /// </summary>
    public string BuildEmpathyPromptSection()
    {
        if (Mode == ConversationMode.Language)
        {
            return string.Empty;
        }

        var empathyRatio = Gauge;
        return $"""

            [Empathy Mode Active]
            현재 공감 게이지: {Gauge}
            응답의 {empathyRatio}%는 감정 공감으로 구성할 것.
            문법 교정, Code-Switching 분석, 언어 레벨 평가 절대 금지.
            """;
    }

    /// <summary>ManualOverride 중인지 여부. 자동 제안 표시 여부 결정에 사용.</summary>
    public bool IsManualOverride => ControlMode == GaugeControlMode.ManualOverride;
}
