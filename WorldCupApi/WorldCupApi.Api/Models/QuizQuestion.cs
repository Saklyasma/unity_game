using WorldCupApi.Api.Data;

namespace WorldCupApi.Api.Models;

/// <summary>
/// A defensive-quiz question tied to a country (mirrors the QuizData.json structure
/// consumed by QuizManager in the Unity client).
/// </summary>
public class QuizQuestion : IEntity
{
    public int Id { get; set; }

    /// <summary>Id of the Country this question is about.</summary>
    public int CountryId { get; set; }

    /// <summary>Language code: "ar", "en" or "fr".</summary>
    public string Language { get; set; } = "ar";

    public string Question { get; set; } = string.Empty;

    public List<string> Answers { get; set; } = new();

    /// <summary>Index into Answers that is correct.</summary>
    public int CorrectIndex { get; set; }

    /// <summary>Optional Resources path to the voice-over clip (e.g. "voices/ne-10-ar").</summary>
    public string? AudioResource { get; set; }
}
