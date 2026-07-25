using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using WorldCupApi.Api.Data;

namespace WorldCupApi.Api.Services;

public interface IGeminiClient
{
    Task<string> GenerateReplyAsync(string systemPrompt, string userMessage, CancellationToken cancellationToken = default);
}

/// <summary>
/// Typed HttpClient wrapper around the Gemini "generateContent" REST endpoint. Registered via
/// AddHttpClient&lt;GeminiClient&gt; in Program.cs (BaseAddress = https://generativelanguage.googleapis.com/).
/// The API key is read from AiTutorSettings:Gemini:ApiKey (dotnet user-secrets locally, an env var
/// elsewhere) — never hardcoded, never logged.
/// </summary>
public class GeminiClient : IGeminiClient
{
    private readonly HttpClient _http;
    private readonly GeminiSettings _settings;

    public GeminiClient(HttpClient http, IOptions<AiTutorSettings> options)
    {
        _http = http;
        _settings = options.Value.Gemini;
    }

    public async Task<string> GenerateReplyAsync(string systemPrompt, string userMessage, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            return "The AI Tutor isn't fully configured yet — an administrator needs to set the Gemini API key.";
        }

        var requestBody = new GeminiRequest
        {
            SystemInstruction = new GeminiContent { Parts = new() { new GeminiPart { Text = systemPrompt } } },
            Contents = new() { new GeminiContentTurn { Role = "user", Parts = new() { new GeminiPart { Text = userMessage } } } },
            // thinkingBudget=0 disables the model's hidden reasoning tokens — without it, "thinking"
            // models (gemini-2.5+/3.x) can burn the entire MaxOutputTokens budget on invisible
            // reasoning before writing the visible reply, truncating it mid-sentence.
            GenerationConfig = new GeminiGenerationConfig
            {
                Temperature = 0.7,
                MaxOutputTokens = 1024,
                ThinkingConfig = new GeminiThinkingConfig { ThinkingBudget = 0 }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"v1beta/models/{_settings.Model}:generateContent")
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Add("x-goog-api-key", _settings.ApiKey);

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, cancellationToken);
        }
        // A slow/unreachable Gemini endpoint throws TaskCanceledException (via the HttpClient's own
        // 20s Timeout) rather than HttpRequestException — catch both so a network hiccup degrades to
        // the fallback message instead of an unhandled 500 reaching the player.
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException)
        {
            return "Sorry, I couldn't reach the AI coach right now. Please try again in a moment.";
        }

        if (!response.IsSuccessStatusCode)
        {
            return "Sorry, I couldn't reach the AI coach right now. Please try again in a moment.";
        }

        var payload = await response.Content.ReadFromJsonAsync<GeminiGenerateContentResponse>(cancellationToken: cancellationToken);
        var text = payload?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
        return string.IsNullOrWhiteSpace(text) ? "Sorry, I don't have an answer for that right now." : text;
    }

    private class GeminiRequest
    {
        [JsonPropertyName("system_instruction")]
        public GeminiContent SystemInstruction { get; set; } = new();

        [JsonPropertyName("contents")]
        public List<GeminiContentTurn> Contents { get; set; } = new();

        [JsonPropertyName("generationConfig")]
        public GeminiGenerationConfig GenerationConfig { get; set; } = new();
    }

    private class GeminiContentTurn
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        [JsonPropertyName("parts")]
        public List<GeminiPart> Parts { get; set; } = new();
    }

    private class GeminiContent
    {
        [JsonPropertyName("parts")]
        public List<GeminiPart> Parts { get; set; } = new();
    }

    private class GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    private class GeminiGenerationConfig
    {
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("maxOutputTokens")]
        public int MaxOutputTokens { get; set; }

        [JsonPropertyName("thinkingConfig")]
        public GeminiThinkingConfig? ThinkingConfig { get; set; }
    }

    private class GeminiThinkingConfig
    {
        [JsonPropertyName("thinkingBudget")]
        public int ThinkingBudget { get; set; }
    }

    private class GeminiGenerateContentResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    private class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }
}
