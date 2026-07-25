namespace WorldCupApi.Api.Data;

/// <summary>
/// Bound from the "AiTutorSettings" section in appsettings.json — same IOptions&lt;T&gt; convention
/// as MongoDbSettings. Gemini:ApiKey is a real secret: set it via `dotnet user-secrets set
/// "AiTutorSettings:Gemini:ApiKey" "..."` locally, or the AiTutorSettings__Gemini__ApiKey
/// environment variable elsewhere — never commit it to appsettings.*.json.
/// </summary>
public class AiTutorSettings
{
    public GeminiSettings Gemini { get; set; } = new();
    public BktSettings Bkt { get; set; } = new();
    public EloSettings Elo { get; set; } = new();
}

public class GeminiSettings
{
    /// <summary>
    /// "gemini-2.5-flash" (a pinned version) returns 404 "no longer available to new users" for
    /// freshly-created API keys — use the "-latest" alias instead, which Google keeps pointed at
    /// their current recommended flash model (resolves to gemini-3.5-flash as of 2026-07).
    /// </summary>
    public string Model { get; set; } = "gemini-flash-latest";
    public string ApiKey { get; set; } = string.Empty;
}

/// <summary>Bayesian Knowledge Tracing parameters — see KnowledgeTracingService for the formulas.</summary>
public class BktSettings
{
    public double PInit { get; set; } = 0.30;
    public double PTransit { get; set; } = 0.15;
    public double PSlip { get; set; } = 0.10;
    public double PGuess { get; set; } = 0.20;
}

/// <summary>Elo rating parameters — see DifficultyEngine for the formulas.</summary>
public class EloSettings
{
    public double StartingRating { get; set; } = 1200;
    public double KFactor { get; set; } = 24;
}
