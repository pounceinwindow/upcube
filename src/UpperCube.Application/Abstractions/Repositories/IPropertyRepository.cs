using UpperCube.Application.DTOs;
using UpperCube.Domain.Entities;

namespace UpperCube.Application.Abstractions.Repositories;

public interface IPropertyRepository
{
    Task<Property?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<(IReadOnlyList<Property> Items, int TotalCount)> SearchAsync(
        PropertySearchFilter filter,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task AddAsync(Property property, CancellationToken ct = default);

    Task UpdateAsync(Property property, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}