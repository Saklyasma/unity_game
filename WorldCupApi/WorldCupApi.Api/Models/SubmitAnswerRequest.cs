using System.ComponentModel.DataAnnotations;

namespace WorldCupApi.Api.Models;

public class SubmitAnswerRequest
{
    [Required]
    public int PlayerId { get; set; }

    /// <summary>Must be one of the AiTutorTopics keys — validated in the controller.</summary>
    [Required]
    public string Topic { get; set; } = string.Empty;

    public int? QuestionId { get; set; }

    public bool IsCorrect { get; set; }

    [Range(0, int.MaxValue)]
    public int ResponseTimeMs { get; set; }

    /// <summary>Difficulty bucket the question was actually offered at: "Easy" | "Medium" | "Hard" | "Expert".</summary>
    [Required]
    public string DifficultyAttempted { get; set; } = "Medium";
}
