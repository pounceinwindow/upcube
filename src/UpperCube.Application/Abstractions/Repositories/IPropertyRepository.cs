using UpperCube.Application.DTOs;
using UpperCube.Domain.Entities;
using UpperCube.Domain.Enums;

namespace UpperCube.Application.Abstractions.Repositories;

public interface IPropertyRepository
{
    Task<int> CountAsync(CancellationToken ct = default);

    Task<int> CountByStatusAsync(PropertyStatus status, CancellationToken ct = default);

    Task<IReadOnlyList<Property>> GetLatestPublishedAsync(int count, CancellationToken ct = default);

    Task<IReadOnlyList<Property>> GetByAgentIdAsync(string agentId, CancellationToken ct = default);

    Task<IReadOnlyList<Property>> GetByStatusAsync(PropertyStatus status, CancellationToken ct = default);

    Task<Property?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<(IReadOnlyList<Property> Items, int TotalCount)> SearchAsync(
        PropertySearchFilter filter,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task AddAsync(Property property, CancellationToken ct = default);

    Task UpdateAsync(Property property, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    Task IncrementViewsAsync(int id, CancellationToken ct = default);
}