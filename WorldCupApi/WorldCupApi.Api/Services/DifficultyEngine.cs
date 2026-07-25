using Microsoft.Extensions.Options;
using WorldCupApi.Api.Data;

namespace WorldCupApi.Api.Services;

public interface IDifficultyEngine
{
    /// <summary>Standard Elo update: currentRating vs. a virtual opponent rated at difficultyAttempted's level.</summary>
    double UpdateElo(double currentRating, string difficultyAttempted, bool correct);

    /// <summary>Maps a continuous Elo rating to a display bucket.</summary>
    string BucketFromRating(double rating);

    /// <summary>XP/level for one answered question. Level = floor(sqrt(xp/50)) + 1.</summary>
    (int xpGained, int newXp, int newLevel, bool leveledUp) ApplyXp(int currentXp, string difficultyAttempted, bool correct);

    int ApplyCoins(int currentCoins, bool correct);
}

/// <summary>
/// Elo-based difficulty adaptation. Each answered question is treated as a "match" against a
/// virtual opponent whose rating represents the question's stated difficulty. A 3-in-a-row streak
/// (tracked on TopicMastery, reset by the caller once consumed) nudges the rating an extra +/-10 —
/// the spec's explicit "bump after a streak of correct, drop after repeated failures" rule, layered
/// on top of the per-answer Elo math rather than replacing it.
/// </summary>
public class DifficultyEngine : IDifficultyEngine
{
    private static readonly IReadOnlyDictionary<string, double> OpponentRatings = new Dictionary<string, double>
    {
        ["Easy"] = 1000,
        ["Medium"] = 1200,
        ["Hard"] = 1400,
        ["Expert"] = 1600
    };

    private static readonly string[] BucketMultiplierOrder = { "Easy", "Medium", "Hard", "Expert" };

    private readonly EloSettings _settings;

    public DifficultyEngine(IOptions<AiTutorSettings> options)
    {
        _settings = options.Value.Elo;
    }

    public double UpdateElo(double currentRating, string difficultyAttempted, bool correct)
    {
        var opponentRating = OpponentRatings.TryGetValue(difficultyAttempted, out var rating) ? rating : 1200;
        var expected = 1.0 / (1.0 + Math.Pow(10, (opponentRating - currentRating) / 400.0));
        var actual = correct ? 1.0 : 0.0;
        return currentRating + _settings.KFactor * (actual - expected);
    }

    public string BucketFromRating(double rating) => rating switch
    {
        < 1100 => "Easy",
        < 1300 => "Medium",
        < 1500 => "Hard",
        _ => "Expert"
    };

    public (int xpGained, int newXp, int newLevel, bool leveledUp) ApplyXp(int currentXp, string difficultyAttempted, bool correct)
    {
        var multiplierIndex = Array.IndexOf(BucketMultiplierOrder, difficultyAttempted);
        var multiplier = multiplierIndex >= 0 ? multiplierIndex : 1;
        var xpGained = correct ? 10 + 5 * multiplier : 2;
        var newXp = currentXp + xpGained;
        var oldLevel = LevelFromXp(currentXp);
        var newLevel = LevelFromXp(newXp);
        return (xpGained, newXp, newLevel, newLevel > oldLevel);
    }

    public int ApplyCoins(int currentCoins, bool correct) => currentCoins + (correct ? 5 : 1);

    private static int LevelFromXp(int xp) => (int)Math.Floor(Math.Sqrt(xp / 50.0)) + 1;
}
