using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;

namespace WorldCupApi.Api.Controllers;

/// <summary>Score predictions submitted by players.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PredictionsController : ControllerBase
{
    private readonly IWorldCupDataStore _store;

    public PredictionsController(IWorldCupDataStore store)
    {
        _store = store;
    }

    /// <summary>Get every prediction submitted so far.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PredictionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PredictionResponse>>> GetPredictions()
    {
        return Ok(await _store.GetPredictionsAsync());
    }

    /// <summary>Submit a score prediction for a match.</summary>
    /// <remarks>The request body is pre-filled with a ready-to-run example in Swagger UI — just hit Execute.</remarks>
    /// <param name="request">Prediction payload.</param>
    [HttpPost]
    [SwaggerRequestExample(typeof(PredictionRequest), typeof(PredictionRequestExample))]
    [ProducesResponseType(typeof(PredictionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PredictionResponse>> CreatePrediction(PredictionRequest request)
    {
        if (await _store.GetMatchAsync(request.MatchId) is null)
        {
            return NotFound($"Match {request.MatchId} does not exist.");
        }

        var created = await _store.AddPredictionAsync(request);
        return CreatedAtAction(nameof(GetPredictions), new { id = created.Id }, created);
    }
}

/// <summary>Provides the sample payload shown by default in Swagger UI's "Try it out" panel.</summary>
public class PredictionRequestExample : IExamplesProvider<PredictionRequest>
{
    public PredictionRequest GetExamples() => new()
    {
        MatchId = 1,
        PlayerName = "Souhaila",
        PredictedHomeScore = 2,
        PredictedAwayScore = 1
    };
}
