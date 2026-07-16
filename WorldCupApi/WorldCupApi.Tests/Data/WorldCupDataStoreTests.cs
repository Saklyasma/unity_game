using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;

namespace WorldCupApi.Tests.Data;

[Collection("Mongo")]
public class WorldCupDataStoreTests
{
    private readonly WorldCupDataStore _store;

    public WorldCupDataStoreTests(MongoTestFixture fixture)
    {
        var db = fixture.CreateDatabase();
        var teams = new MongoRepository<Team>(db, "teams");
        var matches = new MongoRepository<Match>(db, "matches");
        var predictions = new MongoRepository<PredictionResponse>(db, "predictions");

        TestSeed.SeedTeamsAndMatchesAsync(teams, matches).GetAwaiter().GetResult();
        _store = new WorldCupDataStore(teams, matches, predictions);
    }

    [Fact]
    public async Task GetTeams_ReturnsAllSeededTeams()
    {
        var teams = await _store.GetTeamsAsync();

        Assert.Equal(8, teams.Count);
        Assert.Contains(teams, t => t.Name == "France" && t.Code == "FRA");
    }

    [Fact]
    public async Task GetTeam_WithExistingId_ReturnsTeam()
    {
        var team = await _store.GetTeamAsync(1);

        Assert.NotNull(team);
        Assert.Equal("France", team!.Name);
    }

    [Fact]
    public async Task GetTeam_WithUnknownId_ReturnsNull()
    {
        var team = await _store.GetTeamAsync(999);

        Assert.Null(team);
    }

    [Fact]
    public async Task GetMatches_ReturnsAllSeededMatches()
    {
        var matches = await _store.GetMatchesAsync();

        Assert.Equal(4, matches.Count);
    }

    [Fact]
    public async Task GetMatch_WithExistingId_ReturnsMatch()
    {
        var match = await _store.GetMatchAsync(1);

        Assert.NotNull(match);
        Assert.Equal(MatchStatus.Finished, match!.Status);
        Assert.Equal(2, match.HomeScore);
    }

    [Fact]
    public async Task GetMatch_WithUnknownId_ReturnsNull()
    {
        var match = await _store.GetMatchAsync(999);

        Assert.Null(match);
    }

    [Fact]
    public async Task GetMatchesByTeam_ReturnsOnlyMatchesInvolvingThatTeam()
    {
        var matches = await _store.GetMatchesByTeamAsync(1);

        Assert.Single(matches);
        Assert.All(matches, m => Assert.True(m.HomeTeamId == 1 || m.AwayTeamId == 1));
    }

    [Fact]
    public async Task GetMatchesByTeam_WithTeamHavingNoMatches_ReturnsEmpty()
    {
        var matches = await _store.GetMatchesByTeamAsync(999);

        Assert.Empty(matches);
    }

    [Fact]
    public async Task AddPrediction_StoresAndReturnsItWithGeneratedId()
    {
        var request = new PredictionRequest
        {
            MatchId = 1,
            PlayerName = "Souhaila",
            PredictedHomeScore = 2,
            PredictedAwayScore = 1
        };

        var created = await _store.AddPredictionAsync(request);

        Assert.True(created.Id > 0);
        Assert.Equal(request.MatchId, created.MatchId);
        Assert.Equal(request.PlayerName, created.PlayerName);
        Assert.Contains(await _store.GetPredictionsAsync(), p => p.Id == created.Id);
    }

    [Fact]
    public async Task AddPrediction_AssignsIncrementingIds()
    {
        var first = await _store.AddPredictionAsync(new PredictionRequest { MatchId = 1, PlayerName = "A", PredictedHomeScore = 1, PredictedAwayScore = 0 });
        var second = await _store.AddPredictionAsync(new PredictionRequest { MatchId = 1, PlayerName = "B", PredictedHomeScore = 0, PredictedAwayScore = 0 });

        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(first.Id + 1, second.Id);
    }
}
