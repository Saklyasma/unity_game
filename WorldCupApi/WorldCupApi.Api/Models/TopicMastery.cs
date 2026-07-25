namespace WorldCupApi.Api.Models;

/// <summary>
/// Bayesian Knowledge Tracing state for one topic, embedded inside PlayerProfile.
/// MasteryProbability is the current P(L) posterior — updated after every answered question.
/// </summary>
public class TopicMastery
{
    public string Topic { get; set; } = string.Empty;

    /// <summary>Current P(L) — probability the player has mastered this topic. Clamped to [0.01, 0.99].</summary>
    public double MasteryProbability { get; set; }

    public int Attempts { get; set; }
    public int Successes { get; set; }
    public int Failures { get; set; }

    /// <summary>Drives the DifficultyEngine's "bump difficulty up" rule.</summary>
    public int ConsecutiveCorrectStreak { get; set; }

    /// <summary>Drives the DifficultyEngine's "drop difficulty down" rule.</summary>
    public int ConsecutiveWrongStreak { get; set; }

    public DateTime? LastAttemptUtc { get; set; }
}
