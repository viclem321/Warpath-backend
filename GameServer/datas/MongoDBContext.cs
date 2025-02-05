using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace GameServer.Datas;



public class MongoDBContext
{
    private readonly IMongoDatabase _database;

    public MongoDBContext(IOptions<MongoDBSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }
}

public class MongoDBSettings
{
    public string ConnectionString { get; set; } = "";
    public string DatabaseName { get; set; } = "";
}
