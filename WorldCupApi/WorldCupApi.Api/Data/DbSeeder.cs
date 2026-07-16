using Microsoft.Extensions.DependencyInjection;
using WorldCupApi.Api.Models;

namespace WorldCupApi.Api.Data;

/// <summary>
/// Seeds MongoDB with sample data on first run, so every endpoint stays
/// testable straight from Swagger UI — matching the old in-memory store's UX.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var teams = services.GetRequiredService<IRepository<Team>>();
        var matches = services.GetRequiredService<IRepository<Match>>();
        var countries = services.GetRequiredService<IRepository<Country>>();
        var quizQuestions = services.GetRequiredService<IRepository<QuizQuestion>>();
        var botStats = services.GetRequiredService<IRepository<BotStats>>();

        await SeedTeamsAndMatchesAsync(teams, matches);
        await SeedCountriesAsync(countries);
        await SeedQuizQuestionsAsync(countries, quizQuestions);
        await SeedBotStatsAsync(countries, botStats);
    }

    private static async Task SeedTeamsAndMatchesAsync(IRepository<Team> teams, IRepository<Match> matches)
    {
        if ((await teams.GetAllAsync()).Count > 0)
        {
            return;
        }

        var seeded = new[]
        {
            new Team { Name = "France",      Code = "FRA", Group = "A", FlagEmoji = "🇫🇷" },
            new Team { Name = "Tunisia",     Code = "TUN", Group = "A", FlagEmoji = "🇹🇳" },
            new Team { Name = "Brazil",      Code = "BRA", Group = "B", FlagEmoji = "🇧🇷" },
            new Team { Name = "Switzerland", Code = "SUI", Group = "B", FlagEmoji = "🇨🇭" },
            new Team { Name = "Argentina",   Code = "ARG", Group = "C", FlagEmoji = "🇦🇷" },
            new Team { Name = "Poland",      Code = "POL", Group = "C", FlagEmoji = "🇵🇱" },
            new Team { Name = "Morocco",     Code = "MAR", Group = "D", FlagEmoji = "🇲🇦" },
            new Team { Name = "Spain",       Code = "ESP", Group = "D", FlagEmoji = "🇪🇸" },
        };

        var insertedTeams = new List<Team>();
        foreach (var team in seeded)
        {
            insertedTeams.Add(await teams.InsertAsync(team));
        }

        var t = insertedTeams;
        var seededMatches = new[]
        {
            new Match { HomeTeamId = t[0].Id, AwayTeamId = t[1].Id, KickOffUtc = new DateTime(2026, 6, 12, 18, 0, 0, DateTimeKind.Utc), Stadium = "Groupama Stadium", HomeScore = 2, AwayScore = 1, Status = MatchStatus.Finished },
            new Match { HomeTeamId = t[2].Id, AwayTeamId = t[3].Id, KickOffUtc = new DateTime(2026, 6, 13, 15, 0, 0, DateTimeKind.Utc), Stadium = "Allianz Arena", HomeScore = null, AwayScore = null, Status = MatchStatus.Scheduled },
            new Match { HomeTeamId = t[4].Id, AwayTeamId = t[5].Id, KickOffUtc = new DateTime(2026, 6, 14, 20, 0, 0, DateTimeKind.Utc), Stadium = "MetLife Stadium", HomeScore = 1, AwayScore = 1, Status = MatchStatus.Live },
            new Match { HomeTeamId = t[6].Id, AwayTeamId = t[7].Id, KickOffUtc = new DateTime(2026, 6, 15, 17, 0, 0, DateTimeKind.Utc), Stadium = "Estadio Azteca", HomeScore = null, AwayScore = null, Status = MatchStatus.Scheduled },
        };

        foreach (var match in seededMatches)
        {
            await matches.InsertAsync(match);
        }
    }

    /// <summary>Matches ProgressionManager's region layout in the Unity client exactly (indices 0-9).</summary>
    private static async Task<IReadOnlyList<Country>> SeedCountriesAsync(IRepository<Country> countries)
    {
        var existing = await countries.GetAllAsync();
        if (existing.Count > 0)
        {
            return existing;
        }

        var seeded = new[]
        {
            new Country { Name = "Algeria",    Code = "ALG", FlagEmoji = "🇩🇿", Continent = "Africa",  IsUnlockedByDefault = true },
            new Country { Name = "Argentine",  Code = "ARG", FlagEmoji = "🇦🇷", Continent = "America" },
            new Country { Name = "Australia",  Code = "AUS", FlagEmoji = "🇦🇺", Continent = "Oceania" },
            new Country { Name = "Brezil",     Code = "BRA", FlagEmoji = "🇧🇷", Continent = "America" },
            new Country { Name = "France",     Code = "FRA", FlagEmoji = "🇫🇷", Continent = "Europe" },
            new Country { Name = "Roussia",    Code = "RUS", FlagEmoji = "🇷🇺", Continent = "Europe" },
            new Country { Name = "Japan",      Code = "JPN", FlagEmoji = "🇯🇵", Continent = "Asia" },
            new Country { Name = "Germany",    Code = "GER", FlagEmoji = "🇩🇪", Continent = "Africa" },
            new Country { Name = "Niger",      Code = "NER", FlagEmoji = "🇳🇪", Continent = "Asia" },
            new Country { Name = "USA",        Code = "USA", FlagEmoji = "🇺🇸", Continent = "America" },
        };

        var inserted = new List<Country>();
        foreach (var country in seeded)
        {
            inserted.Add(await countries.InsertAsync(country));
        }

        return inserted;
    }

    private static async Task SeedQuizQuestionsAsync(IRepository<Country> countries, IRepository<QuizQuestion> quizQuestions)
    {
        if ((await quizQuestions.GetAllAsync()).Count > 0)
        {
            return;
        }

        var niger = (await countries.FindAsync(c => c.Name == "Niger")).FirstOrDefault();
        if (niger is null)
        {
            return;
        }

        await quizQuestions.InsertAsync(new QuizQuestion
        {
            CountryId = niger.Id,
            Language = "ar",
            Question = "ما لون الدائرة الموجودة في علم النيجر؟",
            Answers = new List<string> { "أ) خضراء", "ب) برتقالية", "ج) زرقاء" },
            CorrectIndex = 1,
            AudioResource = "voices/ne-10-ar"
        });
    }

    private static async Task SeedBotStatsAsync(IRepository<Country> countries, IRepository<BotStats> botStats)
    {
        if ((await botStats.GetAllAsync()).Count > 0)
        {
            return;
        }

        foreach (var country in await countries.GetAllAsync())
        {
            await botStats.InsertAsync(new BotStats
            {
                CountryId = country.Id,
                MoveSpeed = 5.5f,
                JumpForce = 9f,
                KickForce = 14f,
                KickRange = 2.0f,
                Difficulty = 0.7f,
                PressureSpeedBoost = 1.25f,
                Aggressive = 0.5f,
                Defensive = 0.3f,
                Possession = 0.4f
            });
        }
    }
}
