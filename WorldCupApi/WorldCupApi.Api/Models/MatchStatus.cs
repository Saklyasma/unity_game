namespace WorldCupApi.Api.Models;

/// <summary>Lifecycle status of a match.</summary>
public enum MatchStatus
{
    /// <summary>Match has not started yet.</summary>
    Scheduled,

    /// <summary>Match is currently being played.</summary>
    Live,

    /// <summary>Match has ended.</summary>
    Finished
}
