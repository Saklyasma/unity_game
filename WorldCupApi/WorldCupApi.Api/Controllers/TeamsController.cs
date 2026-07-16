using Microsoft.AspNetCore.Mvc;
using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;

namespace WorldCupApi.Api.Controllers;

/// <summary>National teams competing in the tournament.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TeamsController : ControllerBase
{
    private readonly IWorldCupDataStore _store;

    public TeamsController(IWorldCupDataStore store)
    {
        _store = store;
    }

    /// <summary>Get all teams.</summary>
    /// <remarks>Returns the full list of seeded national teams, grouped by their group stage letter.</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Team>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Team>>> GetTeams()
    {
        return Ok(await _store.GetTeamsAsync());
    }

    /// <summary>Get a single team by id.</summary>
    /// <param name="id">Team identifier, e.g. 1.</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Team), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Team>> GetTeam(int id)
    {
        var team = await _store.GetTeamAsync(id);
        return team is null ? NotFound() : Ok(team);
    }
}
