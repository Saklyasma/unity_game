namespace WorldCupApi.Api.Models;

/// <summary>A match enriched with readable team names, ready to display without extra lookups.</summary>
public class MatchResponse
{
    /// <summary>Unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Home team.</summary>
    public TeamSummary HomeTeam { get; set; } = null!;

    /// <summary>Away team.</summary>
    public TeamSummary AwayTeam { get; set; } = null!;

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

/// <summary>Minimal team info embedded in match responses.</summary>
public class TeamSummary
{
    /// <summary>Unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Full country name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>3-letter FIFA country code.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Flag emoji.</summary>
    public string FlagEmoji { get; set; } = string.Empty;
}
