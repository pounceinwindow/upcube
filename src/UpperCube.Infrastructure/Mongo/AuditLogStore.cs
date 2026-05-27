using MongoDB.Bson;
using MongoDB.Driver;
using UpperCube.Application.Abstractions.Persistence;
using UpperCube.Domain.Entities;

namespace UpperCube.Infrastructure.Mongo;

public sealed class AuditLogStore(MongoContext context) : IAuditLogStore
{
    private readonly IMongoCollection<AuditLogEntry> collection =
        context.Database.GetCollection<AuditLogEntry>("audit_logs");

    public Task WriteAsync(AuditLogEntry entry, CancellationToken ct = default)
    {
        entry.Id ??= ObjectId.GenerateNewId().ToString();
        return collection.InsertOneAsync(entry, cancellationToken: ct);
    }

    public async Task<IReadOnlyList<AuditLogEntry>> GetRecentAsync(int count, CancellationToken ct = default)
    {
        count = Math.Clamp(count, 1, 200);

        return await collection
            .Find(FilterDefinition<AuditLogEntry>.Empty)
            .SortByDescending(x => x.Timestamp)
            .Limit(count)
            .ToListAsync(ct);
    }
}