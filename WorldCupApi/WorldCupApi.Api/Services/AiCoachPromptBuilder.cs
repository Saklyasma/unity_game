using WorldCupApi.Api.Models;

namespace WorldCupApi.Api.Services;

public interface IAiCoachPromptBuilder
{
    string BuildSystemPrompt(PlayerProfile profile);
}

/// <summary>Assembles the Gemini system prompt from the player's current state, so every reply is personalized.</summary>
public class AiCoachPromptBuilder : IAiCoachPromptBuilder
{
    public string BuildSystemPrompt(PlayerProfile profile)
    {
        var weakTopics = profile.TopicMasteries
            .OrderBy(m => m.MasteryProbability)
            .Take(3)
            .Select(m => m.Topic);

        var strongTopics = profile.TopicMasteries
            .OrderByDescending(m => m.MasteryProbability)
            .Take(2)
            .Select(m => m.Topic);

        var totalAnswers = profile.CorrectAnswers + profile.WrongAnswers;
        var lastQuizScoreText = profile.LastQuizScore.HasValue ? $"{profile.LastQuizScore:P0}" : "n/a";
        var memoryText = string.IsNullOrWhiteSpace(profile.AiMemory.Summary) ? "nothing yet" : profile.AiMemory.Summary;

        return "You are an encouraging AI football tutor and coach for the educational game "
            + "\"Farid Around the World\".\n"
            + $"Player: {profile.Username}, Level {profile.Level}, XP {profile.Xp}, Coins {profile.Coins}.\n"
            + $"Elo rating {profile.EloRating:F0} ({profile.DifficultyBucket} difficulty).\n"
            + $"Accuracy: {profile.Accuracy:P0} over {totalAnswers} questions answered.\n"
            + $"Weak topics (focus encouragement here): {string.Join(", ", weakTopics)}.\n"
            + $"Strong topics: {string.Join(", ", strongTopics)}.\n"
            + $"Countries completed: {profile.CountriesVisited.Count}.\n"
            + $"Current mission country id: {(profile.CurrentMissionCountryId?.ToString() ?? "none")}.\n"
            + $"Last quiz score: {lastQuizScoreText}.\n"
            + $"What you remember about this player: {memoryText}.\n\n"
            + "Respond in the same language the player writes in. Be warm, motivating and concise "
            + "(under 150 words unless a training plan is explicitly requested). Markdown is allowed "
            + "(bold, lists). Never invent specific football facts, scores or dates you are not "
            + "confident about — say you're not sure instead.";
    }
}
