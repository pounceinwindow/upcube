using MongoDB.Bson;
using MongoDB.Driver;
using UpperCube.Application.Abstractions.Persistence;
using UpperCube.Domain.Entities;

namespace UpperCube.Infrastructure.Mongo;

public sealed class ErrorLogStore(MongoContext context) : IErrorLogStore
{
    private readonly IMongoCollection<ErrorLogEntry> collection =
        context.Database.GetCollection<ErrorLogEntry>("error_logs");

    public Task WriteAsync(ErrorLogEntry entry, CancellationToken ct = default)
    {
        entry.Id ??= ObjectId.GenerateNewId().ToString();
        return collection.InsertOneAsync(entry, cancellationToken: ct);
    }

    public async Task<IReadOnlyList<ErrorLogEntry>> GetRecentAsync(int count, CancellationToken ct = default)
    {
        count = Math.Clamp(count, 1, 200);

        return await collection
            .Find(FilterDefinition<ErrorLogEntry>.Empty)
            .SortByDescending(x => x.Timestamp)
            .Limit(count)
            .ToListAsync(ct);
    }
}
