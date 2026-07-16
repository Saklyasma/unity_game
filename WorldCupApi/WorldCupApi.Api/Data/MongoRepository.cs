using System.Linq.Expressions;
using MongoDB.Driver;

namespace WorldCupApi.Api.Data;

/// <summary>Concrete Repository implementation backed by a real MongoDB collection.</summary>
public class MongoRepository<T> : IRepository<T> where T : IEntity
{
    private readonly IMongoCollection<T> _collection;
    private readonly IMongoCollection<CounterDocument> _counters;
    private readonly string _collectionName;

    public MongoRepository(IMongoDatabase database, string collectionName)
    {
        _collectionName = collectionName;
        _collection = database.GetCollection<T>(collectionName);
        _counters = database.GetCollection<CounterDocument>("counters");
    }

    public async Task<T?> GetByIdAsync(int id) =>
        await _collection.Find(e => e.Id == id).FirstOrDefaultAsync();

    public async Task<T?> GetRandomAsync()
    {
        var sample = await _collection.Aggregate()
            .Sample(1)
            .ToListAsync();
        return sample.FirstOrDefault();
    }

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _collection.Find(predicate).ToListAsync();

    public async Task<IReadOnlyList<T>> GetAllAsync() =>
        await _collection.Find(FilterDefinition<T>.Empty).ToListAsync();

    public async Task<T> InsertAsync(T entity)
    {
        entity.Id = await NextIdAsync();
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        var result = await _collection.ReplaceOneAsync(e => e.Id == entity.Id, entity);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _collection.DeleteOneAsync(e => e.Id == id);
        return result.DeletedCount > 0;
    }

    /// <summary>Atomically reserves the next id for this collection (classic MongoDB auto-increment pattern).</summary>
    private async Task<int> NextIdAsync()
    {
        var filter = Builders<CounterDocument>.Filter.Eq(c => c.Id, _collectionName);
        var update = Builders<CounterDocument>.Update.Inc(c => c.Sequence, 1);
        var options = new FindOneAndUpdateOptions<CounterDocument>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        var counter = await _counters.FindOneAndUpdateAsync(filter, update, options);
        return counter.Sequence;
    }
}
