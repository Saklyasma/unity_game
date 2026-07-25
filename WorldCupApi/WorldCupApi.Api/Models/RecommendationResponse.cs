namespace WorldCupApi.Api.Models;

public class RecommendationResponse
{
    public string RecommendedTopic { get; set; } = string.Empty;
    public string RecommendedQuizLabel { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int? RecommendedCountryId { get; set; }
    public string? RecommendedCountryName { get; set; }
    public double Score { get; set; }
}
