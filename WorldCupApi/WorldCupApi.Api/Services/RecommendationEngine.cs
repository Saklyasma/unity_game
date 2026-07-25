using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;

namespace WorldCupApi.Api.Services;

public interface IRecommendationEngine
{
    Task<RecommendationResponse> RecommendAsync(PlayerProfile profile);
}

/// <summary>
/// Content-based recommendation: scores each of the 9 fixed topics by how much the player
/// needs practice there, then separately picks the next unvisited country to explore from
/// the existing countries collection.
/// </summary>
public class RecommendationEngine : IRecommendationEngine
{
    private static readonly IReadOnlyDictionary<string, string> DisplayDifficulty = new Dictionary<string, string>
    {
        ["Easy"] = "Beginner",
        ["Medium"] = "Intermediate",
        ["Hard"] = "Advanced",
        ["Expert"] = "Expert"
    };

    private readonly IRepository<QuizAttemptRecord> _attempts;
    private readonly IRepository<Country> _countries;

    public RecommendationEngine(IRepository<QuizAttemptRecord> attempts, IRepository<Country> countries)
    {
        _attempts = attempts;
        _countries = countries;
    }

    public async Task<RecommendationResponse> RecommendAsync(PlayerProfile profile)
    {
        string bestTopic = AiTutorTopics.All[0];
        double bestScore = double.MinValue;
        double bestRecency = 0;

        foreach (var topic in AiTutorTopics.All)
        {
            var mastery = profile.TopicMasteries.FirstOrDefault(m => m.Topic == topic);
            var masteryProbability = mastery?.MasteryProbability ?? 0.30;
            var attempts = mastery?.Attempts ?? 0;
            var failures = mastery?.Failures ?? 0;

            var recentAttempts = (await _attempts.FindAsync(a => a.PlayerId == profile.Id && a.Topic == topic))
                .OrderByDescending(a => a.SubmittedAtUtc)
                .Take(5)
                .ToList();

            var recencyOfFailure = recentAttempts.Count == 0
                ? 0
                : (double)recentAttempts.Count(a => !a.IsCorrect) / recentAttempts.Count;
            var failureRate = attempts == 0 ? 0 : (double)failures / attempts;
            var attemptSaturation = Math.Min(attempts / 20.0, 1.0);

            var score = 0.45 * (1 - masteryProbability)
                      + 0.25 * recencyOfFailure
                      + 0.20 * failureRate
                      - 0.10 * attemptSaturation;

            if (score > bestScore)
            {
                bestScore = score;
                bestTopic = topic;
                bestRecency = recencyOfFailure;
            }
        }

        var difficultyLabel = bestRecency > 0.6
            ? "Beginner"
            : DisplayDifficulty.GetValueOrDefault(profile.DifficultyBucket, "Intermediate");

        var reason = bestRecency > 0.6
            ? $"You've recently missed several {bestTopic} questions — let's rebuild the fundamentals."
            : $"{bestTopic} is your biggest opportunity to improve right now.";

        var (countryId, countryName) = await RecommendCountryAsync(profile);

        return new RecommendationResponse
        {
            RecommendedTopic = bestTopic,
            RecommendedQuizLabel = $"{bestTopic} {difficultyLabel} Quiz",
            Reason = reason,
            RecommendedCountryId = countryId,
            RecommendedCountryName = countryName,
            Score = bestScore
        };
    }

    private async Task<(int? id, string? name)> RecommendCountryAsync(PlayerProfile profile)
    {
        var allCountries = await _countries.GetAllAsync();
        var unvisited = allCountries.Where(c => !profile.CountriesVisited.Contains(c.Id)).ToList();
        if (unvisited.Count == 0)
        {
            return (null, null);
        }

        if (profile.CountriesVisited.Count == 0)
        {
            var first = unvisited.FirstOrDefault(c => c.IsUnlockedByDefault) ?? unvisited.OrderBy(c => c.Id).First();
            return (first.Id, first.Name);
        }

        var lastVisited = allCountries.FirstOrDefault(c => c.Id == profile.CountriesVisited[^1]);
        var next = unvisited.Where(c => c.Continent == lastVisited?.Continent).OrderBy(c => c.Id).FirstOrDefault()
                   ?? unvisited.OrderBy(c => c.Id).First();
        return (next.Id, next.Name);
    }
}
