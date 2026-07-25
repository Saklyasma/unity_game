using WorldCupApi.Api.Data;

namespace WorldCupApi.Api.Models;

/// <summary>
/// The AI Tutor's player profile: XP/coins/progress plus the Bayesian Knowledge Tracing
/// and Elo state that drive recommendations and difficulty. Id equals the frontend's
/// existing mock-auth user id (AuthContext user.id) — it is NOT auto-incremented like
/// other collections. See PlayerProfileRepository for how that id is assigned.
/// </summary>
public class PlayerProfile : IEntity
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public int Xp { get; set; }
    public int Level { get; set; } = 1;
    public int Coins { get; set; }

    public List<int> CountriesVisited { get; set; } = new();
    public List<int> FavoriteTeams { get; set; } = new();
    public List<string> FavoriteCompetitions { get; set; } = new();

    /// <summary>Elo rating driving difficulty adaptation. Starts at 1200 (Medium).</summary>
    public double EloRating { get; set; } = 1200;

    /// <summary>Cached bucket derived from EloRating: "Easy" | "Medium" | "Hard" | "Expert".</summary>
    public string DifficultyBucket { get; set; } = "Medium";

    public int CorrectAnswers { get; set; }
    public int WrongAnswers { get; set; }

    /// <summary>Cached: CorrectAnswers / max(CorrectAnswers+WrongAnswers, 1).</summary>
    public double Accuracy { get; set; }

    public double AverageResponseTimeMs { get; set; }

    /// <summary>One entry per AiTutorTopics topic, eager-seeded when the profile is created.</summary>
    public List<TopicMastery> TopicMasteries { get; set; } = new();

    /// <summary>Pedagogical label distinct from DifficultyBucket — e.g. "Beginner"/"Intermediate"/"Advanced".</summary>
    public string CurrentLearningLevel { get; set; } = "Beginner";

    public AiMemorySnapshot AiMemory { get; set; } = new();

    /// <summary>Last country recommended by RecommendationEngine — the player's "current mission".</summary>
    public int? CurrentMissionCountryId { get; set; }

    public double? LastQuizScore { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
