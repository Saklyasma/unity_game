using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using WorldCupApi.Api.Models;

namespace WorldCupApi.Api.Data;

/// <summary>
/// Ensures MongoDB indexes exist on startup. CreateOneAsync is idempotent — safe to call every run.
/// These indexes are the real enforcement layer for constraints the C# layer only checks
/// optimistically (e.g. one bot profile per country) — a defense against races/duplicate writes.
/// </summary>
public static class IndexInitializer
{
    public static async Task EnsureIndexesAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<IMongoDatabase>();

        // One country name, one document.
        var countries = db.GetCollection<Country>("countries");
        await countries.Indexes.CreateOneAsync(new CreateIndexModel<Country>(
            Builders<Country>.IndexKeys.Ascending(c => c.Name),
            new CreateIndexOptions { Unique = true, Name = "ux_countries_name" }));

        // One FIFA code per team.
        var teams = db.GetCollection<Team>("teams");
        await teams.Indexes.CreateOneAsync(new CreateIndexModel<Team>(
            Builders<Team>.IndexKeys.Ascending(t => t.Code),
            new CreateIndexOptions { Unique = true, Name = "ux_teams_code" }));

        // Fast lookup by country/language (QuizQuestionsController.GetByCountry / GetRandomForCountry).
        var quizQuestions = db.GetCollection<QuizQuestion>("quizQuestions");
        await quizQuestions.Indexes.CreateOneAsync(new CreateIndexModel<QuizQuestion>(
            Builders<QuizQuestion>.IndexKeys.Ascending(q => q.CountryId).Ascending(q => q.Language),
            new CreateIndexOptions { Name = "ix_quizQuestions_country_lang" }));

        // Exactly one AI profile per country — enforced at the DB level, not just in the controller.
        var botStats = db.GetCollection<BotStats>("botStats");
        await botStats.Indexes.CreateOneAsync(new CreateIndexModel<BotStats>(
            Builders<BotStats>.IndexKeys.Ascending(b => b.CountryId),
            new CreateIndexOptions { Unique = true, Name = "ux_botStats_countryId" }));

        // Fast lookup by team (MatchesController.GetMatchesByTeam).
        var matches = db.GetCollection<Match>("matches");
        await matches.Indexes.CreateOneAsync(new CreateIndexModel<Match>(
            Builders<Match>.IndexKeys.Ascending(m => m.HomeTeamId),
            new CreateIndexOptions { Name = "ix_matches_homeTeamId" }));
        await matches.Indexes.CreateOneAsync(new CreateIndexModel<Match>(
            Builders<Match>.IndexKeys.Ascending(m => m.AwayTeamId),
            new CreateIndexOptions { Name = "ix_matches_awayTeamId" }));

        // Fast lookup by match (PredictionsController.CreatePrediction validates MatchId).
        var predictions = db.GetCollection<PredictionResponse>("predictions");
        await predictions.Indexes.CreateOneAsync(new CreateIndexModel<PredictionResponse>(
            Builders<PredictionResponse>.IndexKeys.Ascending(p => p.MatchId),
            new CreateIndexOptions { Name = "ix_predictions_matchId" }));

        // AI Tutor: fast lookup of a player's chat history, oldest-first (AiChatController.GetHistory).
        var aiChatMessages = db.GetCollection<ChatMessage>("aiChatMessages");
        await aiChatMessages.Indexes.CreateOneAsync(new CreateIndexModel<ChatMessage>(
            Builders<ChatMessage>.IndexKeys.Ascending(m => m.PlayerId).Ascending(m => m.CreatedAtUtc),
            new CreateIndexOptions { Name = "ix_aiChatMessages_player_createdAt" }));

        // AI Tutor: fast lookup of a player's recent attempts per topic (RecommendationEngine's recency-of-failure scoring).
        var aiQuizAttempts = db.GetCollection<QuizAttemptRecord>("aiQuizAttempts");
        await aiQuizAttempts.Indexes.CreateOneAsync(new CreateIndexModel<QuizAttemptRecord>(
            Builders<QuizAttemptRecord>.IndexKeys.Ascending(a => a.PlayerId).Ascending(a => a.Topic).Descending(a => a.SubmittedAtUtc),
            new CreateIndexOptions { Name = "ix_aiQuizAttempts_player_topic_submittedAt" }));
    }
}
