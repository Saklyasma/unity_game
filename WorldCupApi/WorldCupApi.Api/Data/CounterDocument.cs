using MongoDB.Bson.Serialization.Attributes;

namespace WorldCupApi.Api.Data;

/// <summary>
/// Backs the auto-increment scheme: MongoDB has no native auto-increment,
/// so each collection gets one counter document tracking its last used id.
/// </summary>
public class CounterDocument
{
    [BsonId]
    public string Id { get; set; } = string.Empty; // collection name

    public int Sequence { get; set; }
}
