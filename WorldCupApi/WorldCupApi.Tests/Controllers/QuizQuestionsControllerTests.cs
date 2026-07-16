using Microsoft.AspNetCore.Mvc;
using WorldCupApi.Api.Controllers;
using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;

namespace WorldCupApi.Tests.Controllers;

[Collection("Mongo")]
public class QuizQuestionsControllerTests
{
    private readonly QuizQuestionsController _controller;
    private readonly int _nigerId;
    private readonly int _tunisiaId;

    public QuizQuestionsControllerTests(MongoTestFixture fixture)
    {
        var db = fixture.CreateDatabase();
        var questions = new MongoRepository<QuizQuestion>(db, "quizQuestions");
        var countries = new MongoRepository<Country>(db, "countries");

        var niger = countries.InsertAsync(new Country { Name = "Niger", Code = "NER" }).GetAwaiter().GetResult();
        var tunisia = countries.InsertAsync(new Country { Name = "Tunisia", Code = "TUN" }).GetAwaiter().GetResult();
        _nigerId = niger.Id;
        _tunisiaId = tunisia.Id;

        _controller = new QuizQuestionsController(questions, countries);
    }

    private QuizQuestionRequest ValidRequest() => new()
    {
        CountryId = _nigerId,
        Language = "ar",
        Question = "ما لون الدائرة الموجودة في علم النيجر؟",
        Answers = new List<string> { "أ) خضراء", "ب) برتقالية", "ج) زرقاء" },
        CorrectIndex = 1
    };

    [Fact]
    public async Task Create_ThenGetById_ReturnsTheQuestion()
    {
        var created = await _controller.Create(ValidRequest());
        var createdResult = Assert.IsType<CreatedAtActionResult>(created.Result);
        var question = Assert.IsType<QuizQuestion>(createdResult.Value);

        var result = await _controller.GetById(question.Id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(1, Assert.IsType<QuizQuestion>(okResult.Value).CorrectIndex);
    }

    [Fact]
    public async Task Create_DoesNotLetClientSetId()
    {
        var created = await _controller.Create(ValidRequest());
        var createdResult = Assert.IsType<CreatedAtActionResult>(created.Result);
        var question = Assert.IsType<QuizQuestion>(createdResult.Value);

        // QuizQuestionRequest has no Id field at all — it's always server-generated.
        Assert.True(question.Id > 0);
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
    public async Task Create_WithCorrectIndexOutOfRange_ReturnsBadRequest()
    {
        var request = ValidRequest();
        request.CorrectIndex = 5; // only 3 answers, valid indices are 0-2

        var result = await _controller.Create(request);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByCountry_FiltersByCountryAndLanguage()
    {
        var arQuestion = ValidRequest();
        var enQuestion = ValidRequest();
        enQuestion.Language = "en";
        var otherCountry = ValidRequest();
        otherCountry.CountryId = _tunisiaId;
        otherCountry.Question = "Other country question";

        await _controller.Create(arQuestion);
        await _controller.Create(enQuestion);
        await _controller.Create(otherCountry);

        var result = await _controller.GetByCountry(_nigerId, "ar");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var questions = Assert.IsAssignableFrom<IEnumerable<QuizQuestion>>(okResult.Value);
        Assert.Single(questions);
    }

    [Fact]
    public async Task GetRandomForCountry_WithNoQuestions_ReturnsNotFound()
    {
        var result = await _controller.GetRandomForCountry(_nigerId, "ar");

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetRandomForCountry_WithQuestions_ReturnsOneOfThem()
    {
        var q1 = ValidRequest();
        var q2 = ValidRequest();
        q2.Question = "Second question";

        await _controller.Create(q1);
        await _controller.Create(q2);

        var result = await _controller.GetRandomForCountry(_nigerId, "ar");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var question = Assert.IsType<QuizQuestion>(okResult.Value);
        Assert.Contains(question.Question, new[] { q1.Question, q2.Question });
    }

    [Fact]
    public async Task Update_WithValidData_PersistsChanges()
    {
        var created = (await _controller.Create(ValidRequest())).Result;
        var question = Assert.IsType<QuizQuestion>(Assert.IsType<CreatedAtActionResult>(created).Value);

        var updateRequest = ValidRequest();
        updateRequest.CorrectIndex = 2;

        var updateResult = await _controller.Update(question.Id, updateRequest);
        var reloaded = await _controller.GetById(question.Id);

        Assert.IsType<NoContentResult>(updateResult);
        var okResult = Assert.IsType<OkObjectResult>(reloaded.Result);
        Assert.Equal(2, Assert.IsType<QuizQuestion>(okResult.Value).CorrectIndex);
    }

    [Fact]
    public async Task Delete_WithExistingId_RemovesTheQuestion()
    {
        var created = (await _controller.Create(ValidRequest())).Result;
        var question = Assert.IsType<QuizQuestion>(Assert.IsType<CreatedAtActionResult>(created).Value);

        var deleteResult = await _controller.Delete(question.Id);
        var reloaded = await _controller.GetById(question.Id);

        Assert.IsType<NoContentResult>(deleteResult);
        Assert.IsType<NotFoundResult>(reloaded.Result);
    }
}
