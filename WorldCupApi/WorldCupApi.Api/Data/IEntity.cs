namespace WorldCupApi.Api.Data;

/// <summary>Contract every MongoDB-backed entity must satisfy so the generic repository can operate on it.</summary>
public interface IEntity
{
    /// <summary>Business identifier (also used as the MongoDB `_id` — BSON supports int32 ids directly).</summary>
    int Id { get; set; }
}
