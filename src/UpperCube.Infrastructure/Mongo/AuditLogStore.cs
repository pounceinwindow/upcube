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
}