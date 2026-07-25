using WorldCupApi.Api.Data;

namespace WorldCupApi.Api.Models;

/// <summary>One turn of AI Tutor chat history, persisted so conversations survive across sessions.</summary>
public class ChatMessage : IEntity
{
    public int Id { get; set; }

    public int PlayerId { get; set; }

    /// <summary>"user" or "model".</summary>
    public string Role { get; set; } = "user";

    public string Content { get; set; } = string.Empty;

    /// <summary>Topic the message was about, if identifiable — used for AiMemorySnapshot bookkeeping.</summary>
    public string? Topic { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
