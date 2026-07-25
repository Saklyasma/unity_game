namespace WorldCupApi.Api.Models;

/// <summary>
/// The fixed set of football-knowledge topics the AI Tutor tracks mastery for.
/// Deliberately separate from the Chatbot widget's own geography/flags/football
/// categories (src/data/chatIntents.js on the frontend) — unrelated taxonomy.
/// </summary>
public static class AiTutorTopics
{
    public const string FifaWorldCup = "FifaWorldCup";
    public const string ChampionsLeague = "ChampionsLeague";
    public const string PremierLeague = "PremierLeague";
    public const string LaLiga = "LaLiga";
    public const string SerieA = "SerieA";
    public const string Bundesliga = "Bundesliga";
    public const string FootballLegends = "FootballLegends";
    public const string NationalTeams = "NationalTeams";
    public const string RulesOfFootball = "RulesOfFootball";

    public static readonly IReadOnlyList<string> All = new[]
    {
        FifaWorldCup, ChampionsLeague, PremierLeague, LaLiga,
        SerieA, Bundesliga, FootballLegends, NationalTeams, RulesOfFootball
    };

    public static bool IsValid(string topic) => All.Contains(topic);
}
