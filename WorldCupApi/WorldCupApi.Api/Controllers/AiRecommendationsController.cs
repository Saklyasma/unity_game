using Microsoft.AspNetCore.Mvc;
using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;
using WorldCupApi.Api.Services;

namespace WorldCupApi.Api.Controllers;

/// <summary>Content-based quiz/country recommendations, computed from the player's current mastery and history.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AiRecommendationsController : ControllerBase
{
    private readonly IPlayerProfileRepository _profiles;
    private readonly IRecommendationEngine _recommendations;
    private readonly AiTutorSettings _settings;

    public AiRecommendationsController(
        IPlayerProfileRepository profiles,
        IRecommendationEngine recommendations,
        Microsoft.Extensions.Options.IOptions<AiTutorSettings> options)
    {
        _profiles = profiles;
        _recommendations = recommendations;
        _settings = options.Value;
    }

    /// <summary>Get the next recommended quiz topic/difficulty and next country to explore.</summary>
    [HttpGet("{playerId:int}")]
    [ProducesResponseType(typeof(RecommendationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RecommendationResponse>> GetRecommendation(int playerId)
    {
        var profile = await _profiles.GetOrCreateAsync(playerId, $"Player{playerId}", _settings.Bkt.PInit);
        return Ok(await _recommendations.RecommendAsync(profile));
    }
}
