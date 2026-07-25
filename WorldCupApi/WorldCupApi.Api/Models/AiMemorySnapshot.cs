namespace WorldCupApi.Api.Models;

/// <summary>
/// A condensed, rolling memory of the player's conversations — kept short so the Gemini
/// prompt doesn't grow unbounded with raw chat history. Updated opportunistically by
/// AiTutorOrchestrator after each chat turn.
/// </summary>
public class AiMemorySnapshot
{
    public string Summary { get; set; } = string.Empty;

    public List<string> RecentTopicsDiscussed { get; set; } = new();

    public DateTime LastUpdatedUtc { get; set; }
}
