using WorldCupApi.Api.Models;

namespace WorldCupApi.Api.Services;

/// <summary>Shared PlayerProfile → response DTO mapping, used by both the orchestrator and PlayerProfilesController.</summary>
public static class PlayerProfileMapper
{
    public static PlayerProfileResponse ToResponse(PlayerProfile p) => new()
    {
        PlayerId = p.Id,
        Username = p.Username,
        Xp = p.Xp,
        Level = p.Level,
        Coins = p.Coins,
        CountriesVisited = p.CountriesVisited,
        FavoriteTeams = p.FavoriteTeams,
        FavoriteCompetitions = p.FavoriteCompetitions,
        EloRating = p.EloRating,
        DifficultyBucket = p.DifficultyBucket,
        CorrectAnswers = p.CorrectAnswers,
        WrongAnswers = p.WrongAnswers,
        Accuracy = p.Accuracy,
        AverageResponseTimeMs = p.AverageResponseTimeMs,
        TopicMasteries = p.TopicMasteries.Select(ToDto).ToList(),
        CurrentLearningLevel = p.CurrentLearningLevel,
        CurrentMissionCountryId = p.CurrentMissionCountryId,
        LastQuizScore = p.LastQuizScore,
        UpdatedAtUtc = p.UpdatedAtUtc
    };

    public static TopicMasteryDto ToDto(TopicMastery m) => new()
    {
        Topic = m.Topic,
        MasteryProbability = m.MasteryProbability,
        Attempts = m.Attempts,
        Successes = m.Successes,
        Failures = m.Failures
    };
}
