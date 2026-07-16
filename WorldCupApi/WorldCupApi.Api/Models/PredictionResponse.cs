using WorldCupApi.Api.Data;

namespace WorldCupApi.Api.Models;

/// <summary>A stored prediction, echoed back after submission or when listed.</summary>
public class PredictionResponse : IEntity
{
    /// <summary>Unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Id of the predicted match.</summary>
    public int MatchId { get; set; }

    /// <summary>Name of the player who made the prediction.</summary>
    public string PlayerName { get; set; } = string.Empty;

    /// <summary>Predicted number of goals for the home team.</summary>
    public int PredictedHomeScore { get; set; }

    /// <summary>Predicted number of goals for the away team.</summary>
    public int PredictedAwayScore { get; set; }

    /// <summary>When the prediction was submitted (UTC).</summary>
    public DateTime SubmittedAtUtc { get; set; }
}
