using System.ComponentModel.DataAnnotations;

namespace WorldCupApi.Api.Models;

/// <summary>Payload to create or update a country. Id is server-generated — never sent by the client.</summary>
public class CountryRequest
{
    [Required]
    [MaxLength(60)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(3)]
    public string Code { get; set; } = string.Empty;

    public string FlagEmoji { get; set; } = string.Empty;

    [Required]
    public string Continent { get; set; } = string.Empty;

    public bool IsUnlockedByDefault { get; set; }
}
