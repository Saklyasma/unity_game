namespace WorldCupApi.Api.Models;

public class SubmitAnswerResponse
{
    public int XpGained { get; set; }
    public int NewXp { get; set; }
    public int NewLevel { get; set; }
    public bool LeveledUp { get; set; }
    public double NewEloRating { get; set; }
    public string NewDifficultyBucket { get; set; } = string.Empty;
    public TopicMasteryDto TopicMastery { get; set; } = new();
    public int NewCoins { get; set; }
}
