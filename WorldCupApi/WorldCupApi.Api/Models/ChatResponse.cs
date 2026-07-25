namespace WorldCupApi.Api.Models;

public class ChatResponse
{
    public string Reply { get; set; } = string.Empty;
    public List<string> SuggestedQuestions { get; set; } = new();
    public DateTime CreatedAtUtc { get; set; }
}
