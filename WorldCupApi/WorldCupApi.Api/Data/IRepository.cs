using System.Linq.Expressions;

namespace WorldCupApi.Api.Data;

/// <summary>
/// Generic data-access abstraction (Repository pattern) — controllers depend on this,
/// never on the MongoDB driver directly, so the persistence engine can change without
/// touching business logic.
/// </summary>
public interface IRepository<T> where T : IEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<T?> GetRandomAsync();
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T> InsertAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
}
