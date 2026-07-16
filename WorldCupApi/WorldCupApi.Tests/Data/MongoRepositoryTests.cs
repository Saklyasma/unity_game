using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;

namespace WorldCupApi.Tests.Data;

/// <summary>Exercises every IRepository&lt;T&gt; method against a real (ephemeral) MongoDB instance.</summary>
[Collection("Mongo")]
public class MongoRepositoryTests
{
    private readonly IRepository<Country> _repo;

    public MongoRepositoryTests(MongoTestFixture fixture)
    {
        _repo = new MongoRepository<Country>(fixture.CreateDatabase(), "countries");
    }

    [Fact]
    public async Task InsertAsync_AssignsAnIncrementingId()
    {
        var first = await _repo.InsertAsync(new Country { Name = "Algeria", Code = "ALG" });
        var second = await _repo.InsertAsync(new Country { Name = "Niger", Code = "NER" });

        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsEntity()
    {
        var inserted = await _repo.InsertAsync(new Country { Name = "Niger", Code = "NER" });

        var found = await _repo.GetByIdAsync(inserted.Id);

        Assert.NotNull(found);
        Assert.Equal("Niger", found!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithUnknownId_ReturnsNull()
    {
        var found = await _repo.GetByIdAsync(999);

        Assert.Null(found);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEveryInsertedEntity()
    {
        await _repo.InsertAsync(new Country { Name = "Algeria", Code = "ALG" });
        await _repo.InsertAsync(new Country { Name = "Niger", Code = "NER" });

        var all = await _repo.GetAllAsync();

        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task FindAsync_FiltersByPredicate()
    {
        await _repo.InsertAsync(new Country { Name = "Algeria", Code = "ALG", Continent = "Africa" });
        await _repo.InsertAsync(new Country { Name = "Niger", Code = "NER", Continent = "Africa" });
        await _repo.InsertAsync(new Country { Name = "France", Code = "FRA", Continent = "Europe" });

        var africa = await _repo.FindAsync(c => c.Continent == "Africa");

        Assert.Equal(2, africa.Count);
        Assert.All(africa, c => Assert.Equal("Africa", c.Continent));
    }

    [Fact]
    public async Task GetRandomAsync_ReturnsOneOfTheInsertedEntities()
    {
        await _repo.InsertAsync(new Country { Name = "Algeria", Code = "ALG" });
        await _repo.InsertAsync(new Country { Name = "Niger", Code = "NER" });

        var random = await _repo.GetRandomAsync();

        Assert.NotNull(random);
        Assert.Contains(random!.Name, new[] { "Algeria", "Niger" });
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var inserted = await _repo.InsertAsync(new Country { Name = "Algeria", Code = "ALG" });
        inserted.Code = "DZ";

        var updated = await _repo.UpdateAsync(inserted);
        var reloaded = await _repo.GetByIdAsync(inserted.Id);

        Assert.True(updated);
        Assert.Equal("DZ", reloaded!.Code);
    }

    [Fact]
    public async Task UpdateAsync_WithUnknownId_ReturnsFalse()
    {
        var updated = await _repo.UpdateAsync(new Country { Id = 999, Name = "Ghost" });

        Assert.False(updated);
    }

    [Fact]
    public async Task DeleteAsync_RemovesTheEntity()
    {
        var inserted = await _repo.InsertAsync(new Country { Name = "Algeria", Code = "ALG" });

        var deleted = await _repo.DeleteAsync(inserted.Id);
        var reloaded = await _repo.GetByIdAsync(inserted.Id);

        Assert.True(deleted);
        Assert.Null(reloaded);
    }

    [Fact]
    public async Task DeleteAsync_WithUnknownId_ReturnsFalse()
    {
        var deleted = await _repo.DeleteAsync(999);

        Assert.False(deleted);
    }
}
