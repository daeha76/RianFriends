using RianFriends.Domain.Conversation;
using RianFriends.Domain.Memory;

namespace RianFriends.Application.Abstractions;

/// <summary>
/// LLM 서비스 추상화 인터페이스.
/// Provider(Claude/GPT/Gemini) 교체 시 Infrastructure 구현체만 변경합니다.
/// Application/Domain 레이어 코드는 이 인터페이스에만 의존합니다.
/// </summary>
public interface ILlmService
{
    /// <summary>
    /// 실시간 대화 응답을 생성합니다 (Flagship 모델 사용).
    /// </summary>
    Task<string> GenerateResponseAsync(
        string systemPrompt,
        Message[] contextMessages,
        FriendMemory[] memories,
        CancellationToken ct = default);

    /// <summary>
    /// 메모리 요약을 생성합니다 (경량 배치 모델 사용, 비용 절감).
    /// </summary>
    Task<string> SummarizeMemoryAsync(
        Message[] messages,
        MemoryLayer targetLayer,
        CancellationToken ct = default);

    /// <summary>
    /// SSE 스트리밍 응답을 생성합니다. MAUI 채팅 화면에서 실시간 타이핑 효과 구현에 사용.
    /// </summary>
    IAsyncEnumerable<string> StreamResponseAsync(
        string systemPrompt,
        Message[] contextMessages,
        FriendMemory[] memories,
        CancellationToken ct = default);
}
