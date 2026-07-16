namespace WorldCupApi.Api.Data;

/// <summary>Bound from the "MongoDbSettings" section in appsettings.json — never hardcode connection strings.</summary>
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}
