using Microsoft.AspNetCore.Mvc;
using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;

namespace WorldCupApi.Api.Controllers;

/// <summary>Fixtures between national teams.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MatchesController : ControllerBase
{
    private readonly IWorldCupDataStore _store;

    public MatchesController(IWorldCupDataStore store)
    {
        _store = store;
    }

    /// <summary>Get all matches.</summary>
    /// <remarks>Returns every seeded fixture with home/away team details resolved.</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MatchResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MatchResponse>>> GetMatches()
    {
        var matches = await _store.GetMatchesAsync();
        var responses = await Task.WhenAll(matches.Select(ToResponseAsync));
        return Ok(responses);
    }

    /// <summary>Get a single match by id.</summary>
    /// <param name="id">Match identifier, e.g. 1.</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MatchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MatchResponse>> GetMatch(int id)
    {
        var match = await _store.GetMatchAsync(id);
        return match is null ? NotFound() : Ok(await ToResponseAsync(match));
    }

    /// <summary>Get every match involving a given team.</summary>
    /// <param name="teamId">Team identifier, e.g. 1.</param>
    [HttpGet("team/{teamId:int}")]
    [ProducesResponseType(typeof(IEnumerable<MatchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<MatchResponse>>> GetMatchesByTeam(int teamId)
    {
        if (await _store.GetTeamAsync(teamId) is null)
        {
            return NotFound();
        }

        var matches = await _store.GetMatchesByTeamAsync(teamId);
        var responses = await Task.WhenAll(matches.Select(ToResponseAsync));
        return Ok(responses);
    }

    private async Task<MatchResponse> ToResponseAsync(Match match)
    {
        var home = (await _store.GetTeamAsync(match.HomeTeamId))!;
        var away = (await _store.GetTeamAsync(match.AwayTeamId))!;

        return new MatchResponse
        {
            Id = match.Id,
            HomeTeam = new TeamSummary { Id = home.Id, Name = home.Name, Code = home.Code, FlagEmoji = home.FlagEmoji },
            AwayTeam = new TeamSummary { Id = away.Id, Name = away.Name, Code = away.Code, FlagEmoji = away.FlagEmoji },
            KickOffUtc = match.KickOffUtc,
            Stadium = match.Stadium,
            HomeScore = match.HomeScore,
            AwayScore = match.AwayScore,
            Status = match.Status
        };
    }
}
