namespace WorldCupApi.Api.Models;

/// <summary>Partial update payload — only non-null fields are applied.</summary>
public class UpdatePlayerProfileRequest
{
    public string? Username { get; set; }
    public List<int>? FavoriteTeams { get; set; }
    public List<string>? FavoriteCompetitions { get; set; }
}
