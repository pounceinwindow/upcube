using Microsoft.EntityFrameworkCore;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Application.DTOs;
using UpperCube.Domain.Entities;
using UpperCube.Domain.Enums;

namespace UpperCube.Infrastructure.Persistence.Repositories;

public sealed class PropertyRepository(AppDbContext dbContext) : IPropertyRepository
{
    public Task<Property?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return dbContext.Properties
            .Include(x => x.Images)
            .Include(x => x.Amenities)
            .ThenInclude(pa => pa.Amenity)
            .Include(x => x.City)
            .Include(x => x.District)
            .Include(x => x.PropertyType)
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<(IReadOnlyList<Property> Items, int TotalCount)> SearchAsync(
        PropertySearchFilter filter,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = dbContext.Properties
            .Include(x => x.Images)
            .AsQueryable();

        if (filter.CityId is not null) query = query.Where(x => x.CityId == filter.CityId);

        if (filter.DistrictId is not null) query = query.Where(x => x.DistrictId == filter.DistrictId);

        if (filter.PropertyTypeId is not null) query = query.Where(x => x.PropertyTypeId == filter.PropertyTypeId);

        if (filter.CategoryId is not null) query = query.Where(x => x.CategoryId == filter.CategoryId);

        if (filter.TransactionType is not null)
            query = query.Where(x => (int)x.TransactionType == filter.TransactionType);

        if (filter.Status is not null) query = query.Where(x => (int)x.Status == filter.Status);

        if (filter.MinPrice is not null) query = query.Where(x => x.Price.Amount >= filter.MinPrice);

        if (filter.MaxPrice is not null) query = query.Where(x => x.Price.Amount <= filter.MaxPrice);

        if (filter.MinArea is not null) query = query.Where(x => x.Area.Value >= filter.MinArea);

        if (filter.MaxArea is not null) query = query.Where(x => x.Area.Value <= filter.MaxArea);

        if (filter.Rooms is not null) query = query.Where(x => x.Rooms == filter.Rooms);

        if (!string.IsNullOrWhiteSpace(filter.Query))
            query = query.Where(x => x.Title.Contains(filter.Query) || x.Description.Contains(filter.Query));

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.PublishedAt ?? x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public Task AddAsync(Property property, CancellationToken ct = default)
    {
        return dbContext.Properties.AddAsync(property, ct).AsTask();
    }

    public Task UpdateAsync(Property property, CancellationToken ct = default)
    {
        dbContext.Properties.Update(property);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var property = await dbContext.Properties.FindAsync([id], ct);
        if (property is not null) dbContext.Properties.Remove(property);
    }

    public async Task<IReadOnlyList<Property>> GetLatestPublishedAsync(int count, CancellationToken ct = default)
    {
        var query = dbContext.Properties
            .Include(x => x.Images)
            .Include(x => x.City)
            .Include(x => x.District)
            .Include(x => x.PropertyType)
            .Where(x => x.Status == PropertyStatus.Published)
            .OrderByDescending(y => y.PublishedAt)
            .Take(count);
        return await query.ToListAsync(ct);
    }

    public async Task IncrementViewsAsync(int id, CancellationToken ct = default)
    {
        var property = await dbContext.Properties.FindAsync([id], ct);
        if (property is not null)
        {
            property.ViewsCount++;
        }
    }
}