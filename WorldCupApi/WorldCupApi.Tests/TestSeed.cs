using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;

namespace WorldCupApi.Tests;

/// <summary>Shared seed data for tests — mirrors the original in-memory store's sample data exactly.</summary>
public static class TestSeed
{
    public static async Task<List<Team>> SeedTeamsAndMatchesAsync(IRepository<Team> teams, IRepository<Match> matches)
    {
        var t = new List<Team>
        {
            await teams.InsertAsync(new Team { Name = "France",      Code = "FRA", Group = "A", FlagEmoji = "🇫🇷" }),
            await teams.InsertAsync(new Team { Name = "Tunisia",     Code = "TUN", Group = "A", FlagEmoji = "🇹🇳" }),
            await teams.InsertAsync(new Team { Name = "Brazil",      Code = "BRA", Group = "B", FlagEmoji = "🇧🇷" }),
            await teams.InsertAsync(new Team { Name = "Switzerland", Code = "SUI", Group = "B", FlagEmoji = "🇨🇭" }),
            await teams.InsertAsync(new Team { Name = "Argentina",   Code = "ARG", Group = "C", FlagEmoji = "🇦🇷" }),
            await teams.InsertAsync(new Team { Name = "Poland",      Code = "POL", Group = "C", FlagEmoji = "🇵🇱" }),
            await teams.InsertAsync(new Team { Name = "Morocco",     Code = "MAR", Group = "D", FlagEmoji = "🇲🇦" }),
            await teams.InsertAsync(new Team { Name = "Spain",       Code = "ESP", Group = "D", FlagEmoji = "🇪🇸" }),
        };

        await matches.InsertAsync(new Match { HomeTeamId = t[0].Id, AwayTeamId = t[1].Id, KickOffUtc = new DateTime(2026, 6, 12, 18, 0, 0, DateTimeKind.Utc), Stadium = "Groupama Stadium", HomeScore = 2, AwayScore = 1, Status = MatchStatus.Finished });
        await matches.InsertAsync(new Match { HomeTeamId = t[2].Id, AwayTeamId = t[3].Id, KickOffUtc = new DateTime(2026, 6, 13, 15, 0, 0, DateTimeKind.Utc), Stadium = "Allianz Arena", HomeScore = null, AwayScore = null, Status = MatchStatus.Scheduled });
        await matches.InsertAsync(new Match { HomeTeamId = t[4].Id, AwayTeamId = t[5].Id, KickOffUtc = new DateTime(2026, 6, 14, 20, 0, 0, DateTimeKind.Utc), Stadium = "MetLife Stadium", HomeScore = 1, AwayScore = 1, Status = MatchStatus.Live });
        await matches.InsertAsync(new Match { HomeTeamId = t[6].Id, AwayTeamId = t[7].Id, KickOffUtc = new DateTime(2026, 6, 15, 17, 0, 0, DateTimeKind.Utc), Stadium = "Estadio Azteca", HomeScore = null, AwayScore = null, Status = MatchStatus.Scheduled });

        return t;
    }
}
