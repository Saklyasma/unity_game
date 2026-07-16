using Microsoft.AspNetCore.Mvc;
using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;

namespace WorldCupApi.Api.Controllers;

/// <summary>Playable opponent countries — admin CRUD, backed by MongoDB.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CountriesController : ControllerBase
{
    private readonly IRepository<Country> _countries;
    private readonly IRepository<QuizQuestion> _quizQuestions;
    private readonly IRepository<BotStats> _botStats;

    public CountriesController(
        IRepository<Country> countries,
        IRepository<QuizQuestion> quizQuestions,
        IRepository<BotStats> botStats)
    {
        _countries = countries;
        _quizQuestions = quizQuestions;
        _botStats = botStats;
    }

    /// <summary>Get all countries.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Country>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Country>>> GetAll()
    {
        return Ok(await _countries.GetAllAsync());
    }

    /// <summary>Get a single country by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Country), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Country>> GetById(int id)
    {
        var country = await _countries.GetByIdAsync(id);
        return country is null ? NotFound() : Ok(country);
    }

    /// <summary>Create a new country.</summary>
    /// <remarks>Id is server-generated — do not send it. Name must be unique.</remarks>
    [HttpPost]
    [ProducesResponseType(typeof(Country), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Country>> Create(CountryRequest request)
    {
        if ((await _countries.FindAsync(c => c.Name == request.Name)).Count > 0)
        {
            return Conflict($"A country named '{request.Name}' already exists.");
        }

        var created = await _countries.InsertAsync(ToEntity(request));
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Update an existing country.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, CountryRequest request)
    {
        var entity = ToEntity(request);
        entity.Id = id;
        var updated = await _countries.UpdateAsync(entity);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>
    /// Delete a country. Cascades: also deletes every quiz question and bot stats
    /// profile that referenced it, so nothing is left pointing at a country that no longer exists.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _countries.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        foreach (var question in await _quizQuestions.FindAsync(q => q.CountryId == id))
        {
            await _quizQuestions.DeleteAsync(question.Id);
        }

        foreach (var stats in await _botStats.FindAsync(s => s.CountryId == id))
        {
            await _botStats.DeleteAsync(stats.Id);
        }

        return NoContent();
    }

    private static Country ToEntity(CountryRequest request) => new()
    {
        Name = request.Name,
        Code = request.Code,
        FlagEmoji = request.FlagEmoji,
        Continent = request.Continent,
        IsUnlockedByDefault = request.IsUnlockedByDefault
    };
}
