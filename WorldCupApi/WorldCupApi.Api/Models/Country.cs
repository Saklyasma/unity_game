using WorldCupApi.Api.Data;

namespace WorldCupApi.Api.Models;

/// <summary>A playable opponent country (mirrors CountryFlag / ProgressionManager in the Unity client).</summary>
public class Country : IEntity
{
    /// <summary>Unique identifier — matches the country index used by ProgressionManager in Unity.</summary>
    public int Id { get; set; }

    /// <summary>Full country name (e.g. "Niger").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>3-letter code (e.g. "NER").</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Flag emoji, handy for quick display.</summary>
    public string FlagEmoji { get; set; } = string.Empty;

    /// <summary>Region grouping used for progression unlocks (Africa, America, Oceania, Europe, Asia).</summary>
    public string Continent { get; set; } = string.Empty;

    /// <summary>True only for the very first country (unlocked with no prerequisite).</summary>
    public bool IsUnlockedByDefault { get; set; }
}
