using WorldCupApi.Api.Data;

namespace WorldCupApi.Api.Models;

/// <summary>A national team competing in the tournament.</summary>
public class Team : IEntity
{
    /// <summary>Unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Full country name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>3-letter FIFA country code (e.g. "FRA").</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Group letter in the group stage (e.g. "A").</summary>
    public string Group { get; set; } = string.Empty;

    /// <summary>Flag emoji, handy for quick visual identification in responses.</summary>
    public string FlagEmoji { get; set; } = string.Empty;
}
