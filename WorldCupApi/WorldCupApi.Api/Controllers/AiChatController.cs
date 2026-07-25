using Microsoft.AspNetCore.Mvc;
using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;
using WorldCupApi.Api.Services;

namespace WorldCupApi.Api.Controllers;

/// <summary>AI Tutor chat — sends a message to Gemini with the player's context, persists both turns.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AiChatController : ControllerBase
{
    private readonly IAiTutorOrchestrator _orchestrator;
    private readonly IRepository<ChatMessage> _messages;

    public AiChatController(IAiTutorOrchestrator orchestrator, IRepository<ChatMessage> messages)
    {
        _orchestrator = orchestrator;
        _messages = messages;
    }

    /// <summary>Send a chat message and get the AI Tutor's reply.</summary>
    [HttpPost("messages")]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatResponse>> SendMessage(ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("message must not be empty.");
        }

        return Ok(await _orchestrator.ChatAsync(request));
    }

    /// <summary>Get recent chat history for a player, oldest first.</summary>
    [HttpGet("messages/{playerId:int}")]
    [ProducesResponseType(typeof(IEnumerable<ChatMessageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetHistory(int playerId, [FromQuery] int take = 20)
    {
        var history = (await _messages.FindAsync(m => m.PlayerId == playerId))
            .OrderByDescending(m => m.CreatedAtUtc)
            .Take(Math.Clamp(take, 1, 100))
            .OrderBy(m => m.CreatedAtUtc)
            .Select(m => new ChatMessageDto { Role = m.Role, Content = m.Content, CreatedAtUtc = m.CreatedAtUtc });

        return Ok(history);
    }
}
