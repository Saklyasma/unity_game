using Microsoft.AspNetCore.Mvc;
using WorldCupApi.Api.Controllers;
using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;

namespace WorldCupApi.Tests.Controllers;

[Collection("Mongo")]
public class TeamsControllerTests
{
    private readonly TeamsController _controller;

    public TeamsControllerTests(MongoTestFixture fixture)
    {
        var db = fixture.CreateDatabase();
        var teams = new MongoRepository<Team>(db, "teams");
        var matches = new MongoRepository<Match>(db, "matches");
        var predictions = new MongoRepository<PredictionResponse>(db, "predictions");

        TestSeed.SeedTeamsAndMatchesAsync(teams, matches).GetAwaiter().GetResult();
        _controller = new TeamsController(new WorldCupDataStore(teams, matches, predictions));
    }

    [Fact]
    public async Task GetTeams_ReturnsOkWithAllTeams()
    {
        var result = await _controller.GetTeams();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var teams = Assert.IsAssignableFrom<IEnumerable<Team>>(okResult.Value);
        Assert.Equal(8, teams.Count());
    }

    [Fact]
    public async Task GetTeam_WithExistingId_ReturnsOkWithTeam()
    {
        var result = await _controller.GetTeam(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var team = Assert.IsType<Team>(okResult.Value);
        Assert.Equal("France", team.Name);
    }

    [Fact]
    public async Task GetTeam_WithUnknownId_ReturnsNotFound()
    {
        var result = await _controller.GetTeam(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }
}
