using Microsoft.AspNetCore.Mvc;
using WorldCupApi.Api.Controllers;
using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;

namespace WorldCupApi.Tests.Controllers;

[Collection("Mongo")]
public class MatchesControllerTests
{
    private readonly MatchesController _controller;

    public MatchesControllerTests(MongoTestFixture fixture)
    {
        var db = fixture.CreateDatabase();
        var teams = new MongoRepository<Team>(db, "teams");
        var matches = new MongoRepository<Match>(db, "matches");
        var predictions = new MongoRepository<PredictionResponse>(db, "predictions");

        TestSeed.SeedTeamsAndMatchesAsync(teams, matches).GetAwaiter().GetResult();
        _controller = new MatchesController(new WorldCupDataStore(teams, matches, predictions));
    }

    [Fact]
    public async Task GetMatches_ReturnsOkWithAllMatchesResolved()
    {
        var result = await _controller.GetMatches();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var matches = Assert.IsAssignableFrom<IEnumerable<MatchResponse>>(okResult.Value).ToList();
        Assert.Equal(4, matches.Count);
        Assert.All(matches, m =>
        {
            Assert.NotNull(m.HomeTeam);
            Assert.NotNull(m.AwayTeam);
        });
    }

    [Fact]
    public async Task GetMatch_WithExistingId_ReturnsOkWithResolvedTeams()
    {
        var result = await _controller.GetMatch(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var match = Assert.IsType<MatchResponse>(okResult.Value);
        Assert.Equal("France", match.HomeTeam.Name);
        Assert.Equal("Tunisia", match.AwayTeam.Name);
    }

    [Fact]
    public async Task GetMatch_WithUnknownId_ReturnsNotFound()
    {
        var result = await _controller.GetMatch(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetMatchesByTeam_WithExistingTeam_ReturnsOkWithItsMatches()
    {
        var result = await _controller.GetMatchesByTeam(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var matches = Assert.IsAssignableFrom<IEnumerable<MatchResponse>>(okResult.Value);
        Assert.Single(matches);
    }

    [Fact]
    public async Task GetMatchesByTeam_WithUnknownTeam_ReturnsNotFound()
    {
        var result = await _controller.GetMatchesByTeam(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }
}
