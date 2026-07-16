using Mongo2Go;
using MongoDB.Driver;

namespace WorldCupApi.Tests;

/// <summary>
/// Spins up ONE real (ephemeral) MongoDB instance for the whole test run via Mongo2Go —
/// no external database needed, but tests genuinely hit a live MongoDB, not a mock.
/// Each test class gets its own isolated database name so tests never see each other's data.
/// </summary>
public class MongoTestFixture : IDisposable
{
    private readonly MongoDbRunner _runner;
    private readonly IMongoClient _client;

    public MongoTestFixture()
    {
        _runner = MongoDbRunner.Start(singleNodeReplSet: false);
        _client = new MongoClient(_runner.ConnectionString);
    }

    /// <summary>Returns a fresh, uniquely-named database on the shared ephemeral instance.</summary>
    public IMongoDatabase CreateDatabase() => _client.GetDatabase($"test_{Guid.NewGuid():N}");

    public void Dispose() => _runner.Dispose();
}

[CollectionDefinition("Mongo")]
public class MongoCollection : ICollectionFixture<MongoTestFixture>
{
}
