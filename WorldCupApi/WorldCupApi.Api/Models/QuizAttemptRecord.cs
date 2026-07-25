using WorldCupApi.Api.Data;

namespace WorldCupApi.Api.Models;

/// <summary>
/// One answered AI Tutor quiz question — the audit trail RecommendationEngine's
/// "recency of failure" scoring and DifficultyEngine's Elo history read from.
/// </summary>
public class QuizAttemptRecord : IEntity
{
    public int Id { get; set; }

    public int PlayerId { get; set; }

    public string Topic { get; set; } = string.Empty;

    public int? QuestionId { get; set; }

    /// <summary>Difficulty bucket the question was offered at: "Easy" | "Medium" | "Hard" | "Expert".</summary>
    public string Difficulty { get; set; } = "Medium";

    public bool IsCorrect { get; set; }

    public int ResponseTimeMs { get; set; }

    public double EloBefore { get; set; }
    public double EloAfter { get; set; }

    public double MasteryBefore { get; set; }
    public double MasteryAfter { get; set; }

    public DateTime SubmittedAtUtc { get; set; }
}
