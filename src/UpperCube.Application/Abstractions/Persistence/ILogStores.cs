using UpperCube.Domain.Entities;

namespace UpperCube.Application.Abstractions.Persistence;

public interface IAuditLogStore
{
    Task WriteAsync(AuditLogEntry entry, CancellationToken ct = default);

    Task<IReadOnlyList<AuditLogEntry>> GetRecentAsync(int count, CancellationToken ct = default);
}

public interface IErrorLogStore
{
    Task WriteAsync(ErrorLogEntry entry, CancellationToken ct = default);

    Task<IReadOnlyList<ErrorLogEntry>> GetRecentAsync(int count, CancellationToken ct = default);
}