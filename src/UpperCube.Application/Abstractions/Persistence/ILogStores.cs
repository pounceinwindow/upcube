using UpperCube.Domain.Entities;

namespace UpperCube.Application.Abstractions.Persistence;

public interface IAuditLogStore
{
    Task WriteAsync(AuditLogEntry entry, CancellationToken ct = default);
}

public interface IErrorLogStore
{
    Task WriteAsync(ErrorLogEntry entry, CancellationToken ct = default);
}