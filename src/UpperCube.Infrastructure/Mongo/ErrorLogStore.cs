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
}