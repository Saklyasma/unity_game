using Microsoft.Extensions.Options;
using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;

namespace WorldCupApi.Api.Services;

public interface IKnowledgeTracingService
{
    /// <summary>Mutates mastery in place: applies one Bayesian Knowledge Tracing update for an answered question.</summary>
    void ApplyAnswer(TopicMastery mastery, bool correct);
}

/// <summary>
/// Bayesian Knowledge Tracing. Two steps per answer: (1) condition the current mastery estimate
/// on the observed correct/incorrect evidence via Bayes' rule (accounting for slip/guess), then
/// (2) apply the transition probability (chance of learning from this attempt) regardless of
/// whether the answer was right or wrong.
/// </summary>
public class KnowledgeTracingService : IKnowledgeTracingService
{
    private readonly BktSettings _settings;

    public KnowledgeTracingService(IOptions<AiTutorSettings> options)
    {
        _settings = options.Value.Bkt;
    }

    public void ApplyAnswer(TopicMastery mastery, bool correct)
    {
        var pL = mastery.MasteryProbability;
        var pSlip = _settings.PSlip;
        var pGuess = _settings.PGuess;

        double pEvidence = correct
            ? pL * (1 - pSlip) / (pL * (1 - pSlip) + (1 - pL) * pGuess)
            : pL * pSlip / (pL * pSlip + (1 - pL) * (1 - pGuess));

        var pNext = pEvidence + (1 - pEvidence) * _settings.PTransit;
        mastery.MasteryProbability = Math.Clamp(pNext, 0.01, 0.99);

        mastery.Attempts++;
        if (correct)
        {
            mastery.Successes++;
            mastery.ConsecutiveCorrectStreak++;
            mastery.ConsecutiveWrongStreak = 0;
        }
        else
        {
            mastery.Failures++;
            mastery.ConsecutiveWrongStreak++;
            mastery.ConsecutiveCorrectStreak = 0;
        }

        mastery.LastAttemptUtc = DateTime.UtcNow;
    }
}
