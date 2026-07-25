using MongoDB.Driver;
using WorldCupApi.Api.Models;

namespace WorldCupApi.Api.Data;

/// <summary>Concrete PlayerProfile persistence, backed directly by IMongoCollection (see IPlayerProfileRepository for why).</summary>
public class PlayerProfileRepository : IPlayerProfileRepository
{
    private readonly IMongoCollection<PlayerProfile> _collection;

    public PlayerProfileRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<PlayerProfile>("aiPlayerProfiles");
    }

    public async Task<PlayerProfile> GetOrCreateAsync(int playerId, string username, double initialTopicMastery)
    {
        var now = DateTime.UtcNow;
        var filter = Builders<PlayerProfile>.Filter.Eq(p => p.Id, playerId);
        var update = Builders<PlayerProfile>.Update
            .SetOnInsert(p => p.Id, playerId)
            .SetOnInsert(p => p.Username, username)
            .SetOnInsert(p => p.Xp, 0)
            .SetOnInsert(p => p.Level, 1)
            .SetOnInsert(p => p.Coins, 0)
            .SetOnInsert(p => p.CountriesVisited, new List<int>())
            .SetOnInsert(p => p.FavoriteTeams, new List<int>())
            .SetOnInsert(p => p.FavoriteCompetitions, new List<string>())
            .SetOnInsert(p => p.EloRating, 1200)
            .SetOnInsert(p => p.DifficultyBucket, "Medium")
            .SetOnInsert(p => p.CorrectAnswers, 0)
            .SetOnInsert(p => p.WrongAnswers, 0)
            .SetOnInsert(p => p.Accuracy, 0)
            .SetOnInsert(p => p.AverageResponseTimeMs, 0)
            .SetOnInsert(p => p.TopicMasteries, SeedTopicMasteries(initialTopicMastery))
            .SetOnInsert(p => p.CurrentLearningLevel, "Beginner")
            .SetOnInsert(p => p.AiMemory, new AiMemorySnapshot { LastUpdatedUtc = now })
            .SetOnInsert(p => p.CurrentMissionCountryId, null)
            .SetOnInsert(p => p.LastQuizScore, null)
            .SetOnInsert(p => p.CreatedAtUtc, now)
            .SetOnInsert(p => p.UpdatedAtUtc, now);

        var options = new FindOneAndUpdateOptions<PlayerProfile>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        return await _collection.FindOneAndUpdateAsync(filter, update, options);
    }

    public async Task<PlayerProfile?> GetByIdAsync(int playerId) =>
        await _collection.Find(p => p.Id == playerId).FirstOrDefaultAsync();

    public async Task UpdateAsync(PlayerProfile profile)
    {
        profile.UpdatedAtUtc = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(p => p.Id == profile.Id, profile);
    }

    private static List<TopicMastery> SeedTopicMasteries(double initialMastery) =>
        AiTutorTopics.All.Select(topic => new TopicMastery
        {
            Topic = topic,
            MasteryProbability = initialMastery
        }).ToList();
}
