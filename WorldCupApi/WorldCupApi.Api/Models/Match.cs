using WorldCupApi.Api.Data;

namespace WorldCupApi.Api.Models;

/// <summary>A fixture between two national teams.</summary>
public class Match : IEntity
{
    /// <summary>Unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Id of the home team.</summary>
    public int HomeTeamId { get; set; }

    /// <summary>Id of the away team.</summary>
    public int AwayTeamId { get; set; }

    /// <summary>Kick-off date and time (UTC).</summary>
    public DateTime KickOffUtc { get; set; }

    /// <summary>Stadium where the match is played.</summary>
    public string Stadium { get; set; } = string.Empty;

    /// <summary>Goals scored by the home team, null until the match starts.</summary>
    public int? HomeScore { get; set; }

    /// <summary>Goals scored by the away team, null until the match starts.</summary>
    public int? AwayScore { get; set; }

    /// <summary>Current status of the match.</summary>
    public MatchStatus Status { get; set; }
}
