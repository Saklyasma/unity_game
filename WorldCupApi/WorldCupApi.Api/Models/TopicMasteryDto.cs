namespace WorldCupApi.Api.Models;

/// <summary>Read-only projection of TopicMastery for API responses.</summary>
public class TopicMasteryDto
{
    public string Topic { get; set; } = string.Empty;
    public double MasteryProbability { get; set; }
    public int Attempts { get; set; }
    public int Successes { get; set; }
    public int Failures { get; set; }
}
