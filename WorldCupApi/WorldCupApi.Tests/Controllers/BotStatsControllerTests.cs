using Microsoft.AspNetCore.Mvc;
using WorldCupApi.Api.Controllers;
using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;

namespace WorldCupApi.Tests.Controllers;

[Collection("Mongo")]
public class BotStatsControllerTests
{
    private readonly BotStatsController _controller;
    private readonly int _nigerId;

    public BotStatsControllerTests(MongoTestFixture fixture)
    {
        var db = fixture.CreateDatabase();
        var botStats = new MongoRepository<BotStats>(db, "botStats");
        var countries = new MongoRepository<Country>(db, "countries");

        var niger = countries.InsertAsync(new Country { Name = "Niger", Code = "NER" }).GetAwaiter().GetResult();
        _nigerId = niger.Id;

        _controller = new BotStatsController(botStats, countries);
    }

    private BotStatsRequest ValidRequest() => new() { CountryId = _nigerId, Difficulty = 0.7f };

    [Fact]
    public async Task Create_ThenGetByCountry_ReturnsTheProfile()
    {
        var request = ValidRequest();
        request.Difficulty = 0.9f;
        request.Aggressive = 0.8f;

        await _controller.Create(request);

        var result = await _controller.GetByCountry(_nigerId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var stats = Assert.IsType<BotStats>(okResult.Value);
        Assert.Equal(0.9f, stats.Difficulty);
    }

    [Fact]
    public async Task Create_WithUnknownCountry_ReturnsBadRequest()
    {
        var request = ValidRequest();
        request.CountryId = 999;

        var result = await _controller.Create(request);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_SecondProfileForSameCountry_ReturnsConflict()
    {
        await _controller.Create(ValidRequest());

        var result = await _controller.Create(ValidRequest());

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByCountry_WithNoProfile_ReturnsNotFound()
    {
        var result = await _controller.GetByCountry(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Update_ChangesDifficulty()
    {
        var created = (await _controller.Create(ValidRequest())).Result;
        var stats = Assert.IsType<BotStats>(Assert.IsType<CreatedAtActionResult>(created).Value);

        var updateRequest = ValidRequest();
        updateRequest.Difficulty = 1.0f;
        var updateResult = await _controller.Update(stats.Id, updateRequest);
        var reloaded = await _controller.GetById(stats.Id);

        Assert.IsType<NoContentResult>(updateResult);
        var okResult = Assert.IsType<OkObjectResult>(reloaded.Result);
        Assert.Equal(1.0f, Assert.IsType<BotStats>(okResult.Value).Difficulty);
    }

    [Fact]
    public async Task Update_WithUnknownCountry_ReturnsBadRequest()
    {
        var created = (await _controller.Create(ValidRequest())).Result;
        var stats = Assert.IsType<BotStats>(Assert.IsType<CreatedAtActionResult>(created).Value);

        var updateRequest = ValidRequest();
        updateRequest.CountryId = 999;
        var result = await _controller.Update(stats.Id, updateRequest);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_WithExistingId_RemovesTheProfile()
    {
        var created = (await _controller.Create(ValidRequest())).Result;
        var stats = Assert.IsType<BotStats>(Assert.IsType<CreatedAtActionResult>(created).Value);

        var deleteResult = await _controller.Delete(stats.Id);
        var reloaded = await _controller.GetById(stats.Id);

        Assert.IsType<NoContentResult>(deleteResult);
        Assert.IsType<NotFoundResult>(reloaded.Result);
    }
}
