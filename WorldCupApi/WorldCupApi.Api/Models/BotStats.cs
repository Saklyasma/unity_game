using WorldCupApi.Api.Data;

namespace WorldCupApi.Api.Models;

/// <summary>
/// Server-side twin of the Unity BotStatsData ScriptableObject — lets the admin
/// panel tune a country's AI opponent without rebuilding the game.
/// </summary>
public class BotStats : IEntity
{
    public int Id { get; set; }

    /// <summary>Id of the Country this AI profile belongs to.</summary>
    public int CountryId { get; set; }

    public float MoveSpeed { get; set; } = 5.5f;
    public float JumpForce { get; set; } = 9f;
    public float KickForce { get; set; } = 14f;
    public float KickRange { get; set; } = 2.0f;

    /// <summary>0 = easy, 1 = hard.</summary>
    public float Difficulty { get; set; } = 0.7f;

    public float PressureSpeedBoost { get; set; } = 1.25f;

    /// <summary>Higher = chases more aggressively, presses higher up the field.</summary>
    public float Aggressive { get; set; } = 0.5f;

    /// <summary>Higher = stays back, prioritizes defense over attack.</summary>
    public float Defensive { get; set; } = 0.3f;

    /// <summary>Higher = dribbles toward goal before shooting, keeps possession longer.</summary>
    public float Possession { get; set; } = 0.4f;
}
