using WorldCupApi.Api.Models;

namespace WorldCupApi.Api.Data;

/// <summary>
/// Composes the generic MongoDB repositories into the domain-facing store the
/// controllers consume — controllers stay unaware that MongoDB exists at all.
/// </summary>
public class WorldCupDataStore : IWorldCupDataStore
{
    private readonly IRepository<Team> _teams;
    private readonly IRepository<Match> _matches;
    private readonly IRepository<PredictionResponse> _predictions;

    public WorldCupDataStore(
        IRepository<Team> teams,
        IRepository<Match> matches,
        IRepository<PredictionResponse> predictions)
    {
        _teams = teams;
        _matches = matches;
        _predictions = predictions;
    }

    public Task<IReadOnlyList<Team>> GetTeamsAsync() => _teams.GetAllAsync();

    public Task<Team?> GetTeamAsync(int id) => _teams.GetByIdAsync(id);

    public Task<IReadOnlyList<Match>> GetMatchesAsync() => _matches.GetAllAsync();

    public Task<Match?> GetMatchAsync(int id) => _matches.GetByIdAsync(id);

    public Task<IReadOnlyList<Match>> GetMatchesByTeamAsync(int teamId) =>
        _matches.FindAsync(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId);

    public Task<IReadOnlyList<PredictionResponse>> GetPredictionsAsync() => _predictions.GetAllAsync();

    public Task<PredictionResponse> AddPredictionAsync(PredictionRequest request) =>
        _predictions.InsertAsync(new PredictionResponse
        {
            MatchId = request.MatchId,
            PlayerName = request.PlayerName,
            PredictedHomeScore = request.PredictedHomeScore,
            PredictedAwayScore = request.PredictedAwayScore,
            SubmittedAtUtc = DateTime.UtcNow
        });
}
