using Microsoft.AspNetCore.Mvc;
using WorldCupApi.Api.Models;
using WorldCupApi.Api.Services;

namespace WorldCupApi.Api.Controllers;

/// <summary>Submitting an answered AI Tutor quiz question — runs BKT + Elo + XP/coins in one call.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AiQuizAttemptsController : ControllerBase
{
    private readonly IAiTutorOrchestrator _orchestrator;

    public AiQuizAttemptsController(IAiTutorOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(SubmitAnswerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SubmitAnswerResponse>> SubmitAnswer(SubmitAnswerRequest request)
    {
        if (!AiTutorTopics.IsValid(request.Topic))
        {
            return BadRequest($"'{request.Topic}' is not a recognized topic. Valid topics: {string.Join(", ", AiTutorTopics.All)}.");
        }

        var result = await _orchestrator.SubmitAnswerAsync(request);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}
