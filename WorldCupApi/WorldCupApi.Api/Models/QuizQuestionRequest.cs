using System.ComponentModel.DataAnnotations;

namespace WorldCupApi.Api.Models;

/// <summary>
/// Payload to create or update a quiz question. Deliberately excludes Id (server-generated
/// by the Repository's auto-increment) — mirrors the PredictionRequest/PredictionResponse split.
/// </summary>
public class QuizQuestionRequest
{
    /// <summary>Id of the Country this question is about — must reference an existing country.</summary>
    [Required]
    public int CountryId { get; set; }

    /// <summary>Language code: "ar", "en" or "fr".</summary>
    [Required]
    [RegularExpression("^(ar|en|fr)$", ErrorMessage = "Language must be 'ar', 'en' or 'fr'.")]
    public string Language { get; set; } = "ar";

    [Required]
    [MaxLength(500)]
    public string Question { get; set; } = string.Empty;

    /// <summary>At least 2 answer options.</summary>
    [Required]
    [MinLength(2, ErrorMessage = "A quiz question needs at least 2 answers.")]
    public List<string> Answers { get; set; } = new();

    /// <summary>Index into Answers that is correct — validated against Answers.Count in the controller.</summary>
    [Range(0, int.MaxValue)]
    public int CorrectIndex { get; set; }

    /// <summary>Optional Resources path to the voice-over clip (e.g. "voices/ne-10-ar").</summary>
    public string? AudioResource { get; set; }
}
