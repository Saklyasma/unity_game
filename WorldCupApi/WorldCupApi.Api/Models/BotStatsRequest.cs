using System.ComponentModel.DataAnnotations;

namespace WorldCupApi.Api.Models;

/// <summary>Payload to create or update a bot stats profile. Id is server-generated — never sent by the client.</summary>
public class BotStatsRequest
{
    /// <summary>Id of the Country this AI profile belongs to — must reference an existing country, and each country may only have one profile.</summary>
    [Required]
    public int CountryId { get; set; }

    [Range(0.1, 50)]
    public float MoveSpeed { get; set; } = 5.5f;

    [Range(0.1, 50)]
    public float JumpForce { get; set; } = 9f;

    [Range(0.1, 50)]
    public float KickForce { get; set; } = 14f;

    [Range(0.1, 20)]
    public float KickRange { get; set; } = 2.0f;

    [Range(0, 1)]
    public float Difficulty { get; set; } = 0.7f;

    [Range(0.1, 5)]
    public float PressureSpeedBoost { get; set; } = 1.25f;

    [Range(0, 1)]
    public float Aggressive { get; set; } = 0.5f;

    [Range(0, 1)]
    public float Defensive { get; set; } = 0.3f;

    [Range(0, 1)]
    public float Possession { get; set; } = 0.4f;
}
