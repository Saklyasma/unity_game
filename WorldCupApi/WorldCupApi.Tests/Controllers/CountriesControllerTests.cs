using Microsoft.AspNetCore.Mvc;
using WorldCupApi.Api.Controllers;
using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;

namespace WorldCupApi.Tests.Controllers;

[Collection("Mongo")]
public class CountriesControllerTests
{
    private readonly CountriesController _controller;
    private readonly IRepository<QuizQuestion> _quizQuestions;
    private readonly IRepository<BotStats> _botStats;

    public CountriesControllerTests(MongoTestFixture fixture)
    {
        var db = fixture.CreateDatabase();
        var countries = new MongoRepository<Country>(db, "countries");
        _quizQuestions = new MongoRepository<QuizQuestion>(db, "quizQuestions");
        _botStats = new MongoRepository<BotStats>(db, "botStats");
        _controller = new CountriesController(countries, _quizQuestions, _botStats);
    }

    private static CountryRequest ValidRequest(string name = "Niger") => new()
    {
        Name = name,
        Code = "NER",
        Continent = "Asia"
    };

    [Fact]
    public async Task Create_ThenGetById_ReturnsTheCountry()
    {
        var created = await _controller.Create(ValidRequest());
        var createdResult = Assert.IsType<CreatedAtActionResult>(created.Result);
        var country = Assert.IsType<Country>(createdResult.Value);

        var result = await _controller.GetById(country.Id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal("Niger", Assert.IsType<Country>(okResult.Value).Name);
    }

    [Fact]
    public async Task Create_WithDuplicateName_ReturnsConflict()
    {
        await _controller.Create(ValidRequest());

        var result = await _controller.Create(ValidRequest());

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_WithUnknownId_ReturnsNotFound()
    {
        var result = await _controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetAll_ReturnsEveryCreatedCountry()
    {
        await _controller.Create(ValidRequest("Algeria"));
        await _controller.Create(ValidRequest("Niger"));

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(2, Assert.IsAssignableFrom<IEnumerable<Country>>(okResult.Value).Count());
    }

    [Fact]
    public async Task Update_WithExistingId_ReturnsNoContentAndPersists()
    {
        var created = (await _controller.Create(ValidRequest())).Result;
        var country = Assert.IsType<Country>(Assert.IsType<CreatedAtActionResult>(created).Value);

        var updateRequest = ValidRequest();
        updateRequest.Code = "NE";
        updateRequest.Continent = "Africa";
        var updateResult = await _controller.Update(country.Id, updateRequest);
        var reloaded = await _controller.GetById(country.Id);

        Assert.IsType<NoContentResult>(updateResult);
        var okResult = Assert.IsType<OkObjectResult>(reloaded.Result);
        Assert.Equal("NE", Assert.IsType<Country>(okResult.Value).Code);
    }

    [Fact]
    public async Task Delete_WithExistingId_RemovesTheCountry()
    {
        var created = (await _controller.Create(ValidRequest())).Result;
        var country = Assert.IsType<Country>(Assert.IsType<CreatedAtActionResult>(created).Value);

        var deleteResult = await _controller.Delete(country.Id);
        var reloaded = await _controller.GetById(country.Id);

        Assert.IsType<NoContentResult>(deleteResult);
        Assert.IsType<NotFoundResult>(reloaded.Result);
    }

    [Fact]
    public async Task Delete_WithUnknownId_ReturnsNotFound()
    {
        var result = await _controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_CascadesToQuizQuestionsAndBotStats()
    {
        var created = (await _controller.Create(ValidRequest())).Result;
        var country = Assert.IsType<Country>(Assert.IsType<CreatedAtActionResult>(created).Value);

        await _quizQuestions.InsertAsync(new QuizQuestion
        {
            CountryId = country.Id,
            Question = "Q",
            Answers = new List<string> { "a", "b" }
        });
        await _botStats.InsertAsync(new BotStats { CountryId = country.Id });

        await _controller.Delete(country.Id);

        Assert.Empty(await _quizQuestions.FindAsync(q => q.CountryId == country.Id));
        Assert.Empty(await _botStats.FindAsync(s => s.CountryId == country.Id));
    }
}
