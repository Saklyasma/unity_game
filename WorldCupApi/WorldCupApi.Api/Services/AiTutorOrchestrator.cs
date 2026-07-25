using Microsoft.Extensions.Options;
using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;

namespace WorldCupApi.Api.Services;

public interface IAiTutorOrchestrator
{
    Task<PlayerProfileResponse> GetOrCreateProfileAsync(int playerId, string? username);
    Task<SubmitAnswerResponse> SubmitAnswerAsync(SubmitAnswerRequest request);
    Task<ChatResponse> ChatAsync(ChatRequest request);
}

/// <summary>
/// The single entry point controllers call for composite flows: submitting an answer touches
/// BKT, Elo, XP/coins and persistence in one transaction-like sequence; chatting touches the
/// profile, the prompt builder, Gemini, and message persistence. Keeping this composition here
/// (rather than in the controllers) means the algorithms in KnowledgeTracingService/DifficultyEngine
/// stay pure and independently testable.
/// </summary>
public class AiTutorOrchestrator : IAiTutorOrchestrator
{
    private readonly IPlayerProfileRepository _profiles;
    private readonly IRepository<ChatMessage> _messages;
    private readonly IRepository<QuizAttemptRecord> _attempts;
    private readonly IKnowledgeTracingService _bkt;
    private readonly IDifficultyEngine _difficulty;
    private readonly IAiCoachPromptBuilder _promptBuilder;
    private readonly IGeminiClient _gemini;
    private readonly AiTutorSettings _settings;

    public AiTutorOrchestrator(
        IPlayerProfileRepository profiles,
        IRepository<ChatMessage> messages,
        IRepository<QuizAttemptRecord> attempts,
        IKnowledgeTracingService bkt,
        IDifficultyEngine difficulty,
        IAiCoachPromptBuilder promptBuilder,
        IGeminiClient gemini,
        IOptions<AiTutorSettings> options)
    {
        _profiles = profiles;
        _messages = messages;
        _attempts = attempts;
        _bkt = bkt;
        _difficulty = difficulty;
        _promptBuilder = promptBuilder;
        _gemini = gemini;
        _settings = options.Value;
    }

    public async Task<PlayerProfileResponse> GetOrCreateProfileAsync(int playerId, string? username)
    {
        var profile = await _profiles.GetOrCreateAsync(playerId, username ?? $"Player{playerId}", _settings.Bkt.PInit);
        return PlayerProfileMapper.ToResponse(profile);
    }

    public async Task<SubmitAnswerResponse> SubmitAnswerAsync(SubmitAnswerRequest request)
    {
        var profile = await _profiles.GetOrCreateAsync(request.PlayerId, $"Player{request.PlayerId}", _settings.Bkt.PInit);

        var mastery = profile.TopicMasteries.FirstOrDefault(m => m.Topic == request.Topic);
        if (mastery is null)
        {
            mastery = new TopicMastery { Topic = request.Topic, MasteryProbability = _settings.Bkt.PInit };
            profile.TopicMasteries.Add(mastery);
        }

        var masteryBefore = mastery.MasteryProbability;
        var eloBefore = profile.EloRating;

        _bkt.ApplyAnswer(mastery, request.IsCorrect);

        var newRating = _difficulty.UpdateElo(profile.EloRating, request.DifficultyAttempted, request.IsCorrect);
        if (mastery.ConsecutiveCorrectStreak >= 3)
        {
            newRating += 10;
            mastery.ConsecutiveCorrectStreak = 0;
        }
        else if (mastery.ConsecutiveWrongStreak >= 3)
        {
            newRating -= 10;
            mastery.ConsecutiveWrongStreak = 0;
        }

        profile.EloRating = newRating;
        profile.DifficultyBucket = _difficulty.BucketFromRating(newRating);

        var (xpGained, newXp, newLevel, leveledUp) = _difficulty.ApplyXp(profile.Xp, request.DifficultyAttempted, request.IsCorrect);
        profile.Xp = newXp;
        profile.Level = newLevel;
        profile.Coins = _difficulty.ApplyCoins(profile.Coins, request.IsCorrect);

        if (request.IsCorrect)
        {
            profile.CorrectAnswers++;
        }
        else
        {
            profile.WrongAnswers++;
        }

        var totalAnswers = profile.CorrectAnswers + profile.WrongAnswers;
        profile.Accuracy = totalAnswers == 0 ? 0 : (double)profile.CorrectAnswers / totalAnswers;
        profile.AverageResponseTimeMs += (request.ResponseTimeMs - profile.AverageResponseTimeMs) / totalAnswers;
        profile.LastQuizScore = profile.Accuracy;

        await _profiles.UpdateAsync(profile);

        await _attempts.InsertAsync(new QuizAttemptRecord
        {
            PlayerId = request.PlayerId,
            Topic = request.Topic,
            QuestionId = request.QuestionId,
            Difficulty = request.DifficultyAttempted,
            IsCorrect = request.IsCorrect,
            ResponseTimeMs = request.ResponseTimeMs,
            EloBefore = eloBefore,
            EloAfter = newRating,
            MasteryBefore = masteryBefore,
            MasteryAfter = mastery.MasteryProbability,
            SubmittedAtUtc = DateTime.UtcNow
        });

        return new SubmitAnswerResponse
        {
            XpGained = xpGained,
            NewXp = profile.Xp,
            NewLevel = profile.Level,
            LeveledUp = leveledUp,
            NewEloRating = profile.EloRating,
            NewDifficultyBucket = profile.DifficultyBucket,
            TopicMastery = PlayerProfileMapper.ToDto(mastery),
            NewCoins = profile.Coins
        };
    }

    public async Task<ChatResponse> ChatAsync(ChatRequest request)
    {
        var profile = await _profiles.GetOrCreateAsync(request.PlayerId, $"Player{request.PlayerId}", _settings.Bkt.PInit);

        var systemPrompt = _promptBuilder.BuildSystemPrompt(profile);
        var reply = await _gemini.GenerateReplyAsync(systemPrompt, request.Message);
        var now = DateTime.UtcNow;

        await _messages.InsertAsync(new ChatMessage
        {
            PlayerId = request.PlayerId,
            Role = "user",
            Content = request.Message,
            CreatedAtUtc = now
        });
        await _messages.InsertAsync(new ChatMessage
        {
            PlayerId = request.PlayerId,
            Role = "model",
            Content = reply,
            CreatedAtUtc = now
        });

        profile.AiMemory.LastUpdatedUtc = now;
        await _profiles.UpdateAsync(profile);

        return new ChatResponse
        {
            Reply = reply,
            SuggestedQuestions = new List<string> { "What should I study?", "Why did I lose?", "Motivate me!" },
            CreatedAtUtc = now
        };
    }
}
