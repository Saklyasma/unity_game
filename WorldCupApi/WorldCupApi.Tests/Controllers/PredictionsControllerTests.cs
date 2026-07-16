using Microsoft.AspNetCore.Mvc;
using WorldCupApi.Api.Controllers;
using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;

namespace WorldCupApi.Tests.Controllers;

[Collection("Mongo")]
public class PredictionsControllerTests
{
    private readonly PredictionsController _controller;

    public PredictionsControllerTests(MongoTestFixture fixture)
    {
        var db = fixture.CreateDatabase();
        var teams = new MongoRepository<Team>(db, "teams");
        var matches = new MongoRepository<Match>(db, "matches");
        var predictions = new MongoRepository<PredictionResponse>(db, "predictions");

        TestSeed.SeedTeamsAndMatchesAsync(teams, matches).GetAwaiter().GetResult();
        _controller = new PredictionsController(new WorldCupDataStore(teams, matches, predictions));
    }

    [Fact]
    public async Task GetPredictions_WhenNoneSubmitted_ReturnsOkWithEmptyList()
    {
        var result = await _controller.GetPredictions();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var predictions = Assert.IsAssignableFrom<IEnumerable<PredictionResponse>>(okResult.Value);
        Assert.Empty(predictions);
    }

    [Fact]
    public async Task CreatePrediction_WithExistingMatch_ReturnsCreatedWithPrediction()
    {
        var request = new PredictionRequest
        {
            MatchId = 1,
            PlayerName = "Souhaila",
            PredictedHomeScore = 2,
            PredictedAwayScore = 1
        };

        var result = await _controller.CreatePrediction(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var prediction = Assert.IsType<PredictionResponse>(createdResult.Value);
        Assert.Equal(request.PlayerName, prediction.PlayerName);
        Assert.Equal(request.MatchId, prediction.MatchId);
    }

    [Fact]
    public async Task CreatePrediction_WithUnknownMatch_ReturnsNotFound()
    {
        var request = new PredictionRequest
        {
            MatchId = 999,
            PlayerName = "Souhaila",
            PredictedHomeScore = 2,
            PredictedAwayScore = 1
        };

        var result = await _controller.CreatePrediction(request);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreatePrediction_ThenGetPredictions_IncludesTheNewPrediction()
    {
        await _controller.CreatePrediction(new PredictionRequest
        {
            MatchId = 1,
            PlayerName = "Souhaila",
            PredictedHomeScore = 2,
            PredictedAwayScore = 1
        });

        var result = await _controller.GetPredictions();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var predictions = Assert.IsAssignableFrom<IEnumerable<PredictionResponse>>(okResult.Value);
        Assert.Single(predictions);
    }
}
