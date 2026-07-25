namespace WorldCupApi.Api.Models;

/// <summary>Dashboard-shaped read model returned by PlayerProfilesController — everything the AIContext needs in one call.</summary>
public class PlayerProfileResponse
{
    public int PlayerId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int Xp { get; set; }
    public int Level { get; set; }
    public int Coins { get; set; }
    public List<int> CountriesVisited { get; set; } = new();
    public List<int> FavoriteTeams { get; set; } = new();
    public List<string> FavoriteCompetitions { get; set; } = new();
    public double EloRating { get; set; }
    public string DifficultyBucket { get; set; } = string.Empty;
    public int CorrectAnswers { get; set; }
    public int WrongAnswers { get; set; }
    public double Accuracy { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public List<TopicMasteryDto> TopicMasteries { get; set; } = new();
    public string CurrentLearningLevel { get; set; } = string.Empty;
    public int? CurrentMissionCountryId { get; set; }
    public double? LastQuizScore { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
