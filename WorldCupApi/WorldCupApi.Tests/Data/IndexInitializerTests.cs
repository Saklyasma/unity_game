using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;

namespace WorldCupApi.Tests.Data;

/// <summary>
/// Proves the unique indexes are real DB-level constraints, not just app-level checks —
/// bypasses the controller entirely and writes straight to the repository.
/// </summary>
[Collection("Mongo")]
public class IndexInitializerTests
{
    private readonly IMongoDatabase _db;

    public IndexInitializerTests(MongoTestFixture fixture)
    {
        _db = fixture.CreateDatabase();
    }

    private async Task EnsureIndexesAsync()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_db);
        await using var provider = services.BuildServiceProvider();
        await IndexInitializer.EnsureIndexesAsync(provider);
    }

    [Fact]
    public async Task BotStats_DuplicateCountryId_ThrowsDuplicateKeyException()
    {
        await EnsureIndexesAsync();
        var botStats = new MongoRepository<BotStats>(_db, "botStats");

        await botStats.InsertAsync(new BotStats { CountryId = 1 });

        var ex = await Assert.ThrowsAsync<MongoWriteException>(
            () => botStats.InsertAsync(new BotStats { CountryId = 1 }));
        Assert.Equal(ServerErrorCategory.DuplicateKey, ex.WriteError.Category);
    }

    [Fact]
    public async Task Countries_DuplicateName_ThrowsDuplicateKeyException()
    {
        await EnsureIndexesAsync();
        var countries = new MongoRepository<Country>(_db, "countries");

        await countries.InsertAsync(new Country { Name = "Niger", Code = "NER" });

        var ex = await Assert.ThrowsAsync<MongoWriteException>(
            () => countries.InsertAsync(new Country { Name = "Niger", Code = "NE2" }));
        Assert.Equal(ServerErrorCategory.DuplicateKey, ex.WriteError.Category);
    }

    [Fact]
    public async Task Teams_DuplicateCode_ThrowsDuplicateKeyException()
    {
        await EnsureIndexesAsync();
        var teams = new MongoRepository<Team>(_db, "teams");

        await teams.InsertAsync(new Team { Name = "France", Code = "FRA" });

        var ex = await Assert.ThrowsAsync<MongoWriteException>(
            () => teams.InsertAsync(new Team { Name = "France B", Code = "FRA" }));
        Assert.Equal(ServerErrorCategory.DuplicateKey, ex.WriteError.Category);
    }
}
