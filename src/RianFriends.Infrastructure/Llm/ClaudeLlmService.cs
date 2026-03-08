using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Conversation;
using RianFriends.Domain.Memory;

namespace RianFriends.Infrastructure.Llm;

/// <summary>
/// Anthropic Claude API 기반 LLM 서비스 구현체.
/// 모델명은 appsettings.json의 Llm:ConversationModel / Llm:BatchModel에서 주입 (하드코딩 금지).
/// </summary>
internal sealed class ClaudeLlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly string _conversationModel;
    private readonly string _batchModel;
    private readonly ILogger<ClaudeLlmService> _logger;

    private const string AnthropicApiVersion = "2023-06-01";
    private const string MessagesEndpoint = "https://api.anthropic.com/v1/messages";

    public ClaudeLlmService(IConfiguration config, HttpClient httpClient, ILogger<ClaudeLlmService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _conversationModel = config["Llm:ConversationModel"]
            ?? throw new InvalidOperationException("Llm:ConversationModel 설정이 필요합니다.");
        _batchModel = config["Llm:BatchModel"]
            ?? throw new InvalidOperationException("Llm:BatchModel 설정이 필요합니다.");

        var apiKey = config["Llm:ApiKey"]
            ?? throw new InvalidOperationException("Llm:ApiKey 설정이 필요합니다.");

        _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", AnthropicApiVersion);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<string> GenerateResponseAsync(
        string systemPrompt,
        Message[] contextMessages,
        FriendMemory[] memories,
        CancellationToken ct = default)
    {
        var requestBody = BuildRequestBody(_conversationModel, systemPrompt, contextMessages, memories, stream: false);
        var response = await _httpClient.PostAsJsonAsync(MessagesEndpoint, requestBody, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ClaudeResponse>(cancellationToken: ct);
        return result?.Content?.FirstOrDefault()?.Text ?? string.Empty;
    }

    public async Task<string> SummarizeMemoryAsync(
        Message[] messages,
        MemoryLayer targetLayer,
        CancellationToken ct = default)
    {
        var summaryPrompt = $"다음 대화를 {targetLayer} 레이어에 적합하게 요약해주세요. 핵심 내용과 감정 기록 위주로 간결하게.";
        var requestBody = BuildRequestBody(_batchModel, summaryPrompt, messages, [], stream: false);

        var response = await _httpClient.PostAsJsonAsync(MessagesEndpoint, requestBody, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ClaudeResponse>(cancellationToken: ct);
        return result?.Content?.FirstOrDefault()?.Text ?? string.Empty;
    }

    public async IAsyncEnumerable<string> StreamResponseAsync(
        string systemPrompt,
        Message[] contextMessages,
        FriendMemory[] memories,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var requestBody = BuildRequestBody(_conversationModel, systemPrompt, contextMessages, memories, stream: true);
        var json = JsonSerializer.Serialize(requestBody);
        var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, MessagesEndpoint) { Content = requestContent };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (line is null)
            {
                break;
            }

            if (string.IsNullOrEmpty(line) || !line.StartsWith("data: "))
            {
                continue;
            }

            var data = line["data: ".Length..];
            if (data == "[DONE]")
            {
                break;
            }

            ClaudeStreamEvent? evt = null;
            try { evt = JsonSerializer.Deserialize<ClaudeStreamEvent>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); }
            catch { continue; }

            var text = evt?.Delta?.Text;
            if (!string.IsNullOrEmpty(text))
            {
                yield return text;
            }
        }

        _logger.LogDebug("LLM 스트리밍 완료");
    }

    private static object BuildRequestBody(
        string model,
        string systemPrompt,
        Message[] contextMessages,
        FriendMemory[] memories,
        bool stream)
    {
        var messages = contextMessages.Select(m => new { role = m.Role, content = m.Content }).ToList();

        // 메모리 컨텍스트 삽입 (최신 ShortTerm + MidTerm)
        if (memories.Length > 0)
        {
            var memorySummary = string.Join("\n\n", memories.Select(m => $"[{m.Layer}] {m.Summary}"));
            systemPrompt = $"{systemPrompt}\n\n[Friend Memory Context]\n{memorySummary}";
        }

        return new
        {
            model,
            max_tokens = 1024,
            system = systemPrompt,
            messages,
            stream
        };
    }

    // ── Response models ────────────────────────────────────────────────────

    private sealed record ClaudeResponse(List<ClaudeContent>? Content);
    private sealed record ClaudeContent(string? Text);
    private sealed record ClaudeStreamEvent(string? Type, ClaudeDelta? Delta);
    private sealed record ClaudeDelta(string? Type, string? Text);
}
