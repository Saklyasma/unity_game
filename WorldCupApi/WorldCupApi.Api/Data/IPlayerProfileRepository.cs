using WorldCupApi.Api.Models;

namespace WorldCupApi.Api.Data;

/// <summary>
/// Dedicated (non-generic) repository for PlayerProfile. Deliberately NOT an IRepository&lt;PlayerProfile&gt;
/// backed by MongoRepository&lt;T&gt;: that generic implementation always assigns a fresh auto-incremented
/// id on insert (see MongoRepository&lt;T&gt;.InsertAsync), but a PlayerProfile's id must equal the
/// externally-supplied playerId (the frontend's mock AuthContext user.id). Keeping this as its own
/// small repository avoids adding an "allow external id" escape hatch to the shared generic contract
/// that every other collection relies on.
/// </summary>
public interface IPlayerProfileRepository
{
    /// <summary>Atomically fetches the profile for playerId, creating a freshly-seeded one if none exists.</summary>
    Task<PlayerProfile> GetOrCreateAsync(int playerId, string username, double initialTopicMastery);

    Task<PlayerProfile?> GetByIdAsync(int playerId);

    Task UpdateAsync(PlayerProfile profile);
}
