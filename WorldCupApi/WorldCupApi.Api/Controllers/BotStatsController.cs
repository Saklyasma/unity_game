using Microsoft.AspNetCore.Mvc;
using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;

namespace WorldCupApi.Api.Controllers;

/// <summary>AI opponent tuning per country — admin CRUD, backed by MongoDB.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BotStatsController : ControllerBase
{
    private readonly IRepository<BotStats> _botStats;
    private readonly IRepository<Country> _countries;

    public BotStatsController(IRepository<BotStats> botStats, IRepository<Country> countries)
    {
        _botStats = botStats;
        _countries = countries;
    }

    /// <summary>Get all bot stat profiles.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BotStats>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BotStats>>> GetAll()
    {
        return Ok(await _botStats.GetAllAsync());
    }

    /// <summary>Get a single bot stats profile by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BotStats), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BotStats>> GetById(int id)
    {
        var stats = await _botStats.GetByIdAsync(id);
        return stats is null ? NotFound() : Ok(stats);
    }

    /// <summary>Get the bot stats profile for a given country.</summary>
    [HttpGet("country/{countryId:int}")]
    [ProducesResponseType(typeof(BotStats), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BotStats>> GetByCountry(int countryId)
    {
        var matches = await _botStats.FindAsync(s => s.CountryId == countryId);
        return matches.Count == 0 ? NotFound() : Ok(matches[0]);
    }

    /// <summary>Create a new bot stats profile.</summary>
    /// <remarks>Id is server-generated. countryId must reference an existing country, and each country may only have one profile.</remarks>
    [HttpPost]
    [ProducesResponseType(typeof(BotStats), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BotStats>> Create(BotStatsRequest request)
    {
        if (await _countries.GetByIdAsync(request.CountryId) is null)
        {
            return BadRequest($"Country {request.CountryId} does not exist.");
        }

        if ((await _botStats.FindAsync(s => s.CountryId == request.CountryId)).Count > 0)
        {
            return Conflict($"Country {request.CountryId} already has a bot stats profile.");
        }

        var created = await _botStats.InsertAsync(ToEntity(request));
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Update an existing bot stats profile.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, BotStatsRequest request)
    {
        if (await _countries.GetByIdAsync(request.CountryId) is null)
        {
            return BadRequest($"Country {request.CountryId} does not exist.");
        }

        var entity = ToEntity(request);
        entity.Id = id;
        var updated = await _botStats.UpdateAsync(entity);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Delete a bot stats profile.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _botStats.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    private static BotStats ToEntity(BotStatsRequest request) => new()
    {
        CountryId = request.CountryId,
        MoveSpeed = request.MoveSpeed,
        JumpForce = request.JumpForce,
        KickForce = request.KickForce,
        KickRange = request.KickRange,
        Difficulty = request.Difficulty,
        PressureSpeedBoost = request.PressureSpeedBoost,
        Aggressive = request.Aggressive,
        Defensive = request.Defensive,
        Possession = request.Possession
    };
}
