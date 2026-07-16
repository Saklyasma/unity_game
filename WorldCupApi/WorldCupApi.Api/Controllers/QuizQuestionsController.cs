using Microsoft.AspNetCore.Mvc;
using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;

namespace WorldCupApi.Api.Controllers;

/// <summary>Defensive-quiz questions per country/language — admin CRUD, backed by MongoDB.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class QuizQuestionsController : ControllerBase
{
    private readonly IRepository<QuizQuestion> _questions;
    private readonly IRepository<Country> _countries;

    public QuizQuestionsController(IRepository<QuizQuestion> questions, IRepository<Country> countries)
    {
        _questions = questions;
        _countries = countries;
    }

    /// <summary>Get all quiz questions.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<QuizQuestion>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<QuizQuestion>>> GetAll()
    {
        return Ok(await _questions.GetAllAsync());
    }

    /// <summary>Get a single quiz question by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(QuizQuestion), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuizQuestion>> GetById(int id)
    {
        var question = await _questions.GetByIdAsync(id);
        return question is null ? NotFound() : Ok(question);
    }

    /// <summary>Get every question for a given country, optionally filtered by language.</summary>
    [HttpGet("country/{countryId:int}")]
    [ProducesResponseType(typeof(IEnumerable<QuizQuestion>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<QuizQuestion>>> GetByCountry(int countryId, [FromQuery] string? language)
    {
        var matches = string.IsNullOrWhiteSpace(language)
            ? await _questions.FindAsync(q => q.CountryId == countryId)
            : await _questions.FindAsync(q => q.CountryId == countryId && q.Language == language);
        return Ok(matches);
    }

    /// <summary>Get one random question for a country (mirrors QuizManager.PickRandom in Unity).</summary>
    [HttpGet("country/{countryId:int}/random")]
    [ProducesResponseType(typeof(QuizQuestion), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuizQuestion>> GetRandomForCountry(int countryId, [FromQuery] string language = "ar")
    {
        var candidates = await _questions.FindAsync(q => q.CountryId == countryId && q.Language == language);
        if (candidates.Count == 0)
        {
            return NotFound();
        }

        return Ok(candidates[Random.Shared.Next(candidates.Count)]);
    }

    /// <summary>Create a new quiz question.</summary>
    /// <remarks>Id is server-generated — do not send it. countryId must reference an existing country.</remarks>
    [HttpPost]
    [ProducesResponseType(typeof(QuizQuestion), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<QuizQuestion>> Create(QuizQuestionRequest request)
    {
        var validation = await ValidateAsync(request);
        if (validation is not null)
        {
            return validation;
        }

        var created = await _questions.InsertAsync(ToEntity(request));
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Update an existing quiz question.</summary>
    /// <remarks>countryId must reference an existing country.</remarks>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, QuizQuestionRequest request)
    {
        var validation = await ValidateAsync(request);
        if (validation is not null)
        {
            return validation;
        }

        var entity = ToEntity(request);
        entity.Id = id;
        var updated = await _questions.UpdateAsync(entity);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>
    /// Cross-field checks DataAnnotations can't express: countryId must exist, and correctIndex
    /// must actually be a valid index into answers.
    /// </summary>
    private async Task<ActionResult?> ValidateAsync(QuizQuestionRequest request)
    {
        if (await _countries.GetByIdAsync(request.CountryId) is null)
        {
            return new BadRequestObjectResult($"Country {request.CountryId} does not exist.");
        }

        if (request.CorrectIndex >= request.Answers.Count)
        {
            return new BadRequestObjectResult(
                $"correctIndex ({request.CorrectIndex}) is out of range for {request.Answers.Count} answers.");
        }

        return null;
    }

    private static QuizQuestion ToEntity(QuizQuestionRequest request) => new()
    {
        CountryId = request.CountryId,
        Language = request.Language,
        Question = request.Question,
        Answers = request.Answers,
        CorrectIndex = request.CorrectIndex,
        AudioResource = request.AudioResource
    };

    /// <summary>Delete a quiz question.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _questions.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
