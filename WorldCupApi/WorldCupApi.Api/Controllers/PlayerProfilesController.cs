using Microsoft.AspNetCore.Mvc;
using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;
using WorldCupApi.Api.Services;

namespace WorldCupApi.Api.Controllers;

/// <summary>AI Tutor player profiles — get-or-create semantics, keyed by the frontend's existing mock-auth user id.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PlayerProfilesController : ControllerBase
{
    private readonly IAiTutorOrchestrator _orchestrator;
    private readonly IPlayerProfileRepository _profiles;

    public PlayerProfilesController(IAiTutorOrchestrator orchestrator, IPlayerProfileRepository profiles)
    {
        _orchestrator = orchestrator;
        _profiles = profiles;
    }

    /// <summary>Get the AI Tutor profile for a player, creating a freshly-seeded one on first call.</summary>
    [HttpGet("{playerId:int}")]
    [ProducesResponseType(typeof(PlayerProfileResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PlayerProfileResponse>> GetOrCreate(int playerId, [FromQuery] string? username)
    {
        return Ok(await _orchestrator.GetOrCreateProfileAsync(playerId, username));
    }

    /// <summary>Update editable profile fields (username snapshot, favorite teams/competitions).</summary>
    [HttpPut("{playerId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int playerId, UpdatePlayerProfileRequest request)
    {
        var profile = await _profiles.GetByIdAsync(playerId);
        if (profile is null)
        {
            return NotFound();
        }

        if (request.Username is not null) profile.Username = request.Username;
        if (request.FavoriteTeams is not null) profile.FavoriteTeams = request.FavoriteTeams;
        if (request.FavoriteCompetitions is not null) profile.FavoriteCompetitions = request.FavoriteCompetitions;

        await _profiles.UpdateAsync(profile);
        return NoContent();
    }
}
