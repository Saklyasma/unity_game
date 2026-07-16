using WorldCupApi.Api.Models;

namespace WorldCupApi.Api.Data;

/// <summary>Domain-facing data access for teams, matches and predictions — backed by MongoDB via IRepository&lt;T&gt;.</summary>
public interface IWorldCupDataStore
{
    Task<IReadOnlyList<Team>> GetTeamsAsync();
    Task<Team?> GetTeamAsync(int id);
    Task<IReadOnlyList<Match>> GetMatchesAsync();
    Task<Match?> GetMatchAsync(int id);
    Task<IReadOnlyList<Match>> GetMatchesByTeamAsync(int teamId);
    Task<IReadOnlyList<PredictionResponse>> GetPredictionsAsync();
    Task<PredictionResponse> AddPredictionAsync(PredictionRequest request);
}
