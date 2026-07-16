using System.ComponentModel.DataAnnotations;

namespace WorldCupApi.Api.Models;

/// <summary>Payload to submit a score prediction for a match.</summary>
public class PredictionRequest
{
    /// <summary>Id of the match being predicted.</summary>
    [Required]
    public int MatchId { get; set; }

    /// <summary>Name of the player making the prediction.</summary>
    [Required]
    [MaxLength(60)]
    public string PlayerName { get; set; } = string.Empty;

    /// <summary>Predicted number of goals for the home team.</summary>
    [Required]
    [Range(0, 20)]
    public int PredictedHomeScore { get; set; }

    /// <summary>Predicted number of goals for the away team.</summary>
    [Required]
    [Range(0, 20)]
    public int PredictedAwayScore { get; set; }
}
