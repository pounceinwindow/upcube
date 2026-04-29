using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace UpperCube.Infrastructure.Mongo;

public sealed class MongoContext
{
    public MongoContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Mongo")
                               ?? throw new InvalidOperationException("Connection string 'Mongo' is not configured.");
        var databaseName = configuration["Mongo:DatabaseName"] ?? "realestate_logs";

        var client = new MongoClient(connectionString);
        Database = client.GetDatabase(databaseName);
    }

    public IMongoDatabase Database { get; }
}