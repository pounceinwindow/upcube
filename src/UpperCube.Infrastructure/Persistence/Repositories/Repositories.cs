using Microsoft.EntityFrameworkCore;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Domain.Common;
using UpperCube.Domain.Entities;

namespace UpperCube.Infrastructure.Persistence.Repositories;

public sealed class FavoriteRepository(AppDbContext dbContext) : IFavoriteRepository
{
    public Task<Favorite?> GetAsync(string userId, int propertyId, CancellationToken ct = default)
    {
        return dbContext.Favorites.FirstOrDefaultAsync(x => x.UserId == userId && x.PropertyId == propertyId, ct);
    }

    public async Task<IReadOnlyList<int>> GetUserFavoritePropertyIdsAsync(string userId, CancellationToken ct = default)
    {
        return await dbContext.Favorites
            .Where(f => f.UserId == userId)
            .Select(f => f.PropertyId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Property>> GetUserFavoritesAsync(string userId, CancellationToken ct = default)
    {
        return await dbContext.Favorites
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.AddedAt)
            .Include(f => f.Property)
            .ThenInclude(p => p!.Images)
            .Include(f => f.Property)
            .ThenInclude(p => p!.City)
            .Include(f => f.Property)
            .ThenInclude(p => p!.District)
            .Include(f => f.Property)
            .ThenInclude(p => p!.PropertyType)
            .Select(f => f.Property!)
            .ToListAsync(ct);
    }

    public Task AddAsync(Favorite favorite, CancellationToken ct = default)
    {
        return dbContext.Favorites.AddAsync(favorite, ct).AsTask();
    }

    public Task DeleteAsync(Favorite favorite, CancellationToken ct = default)
    {
        dbContext.Favorites.Remove(favorite);
        return Task.CompletedTask;
    }
}

public sealed class InquiryRepository(AppDbContext dbContext) : IInquiryRepository
{
    public Task<Inquiry?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return dbContext.Inquiries.Include(x => x.Messages).FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public Task AddAsync(Inquiry inquiry, CancellationToken ct = default)
    {
        return dbContext.Inquiries.AddAsync(inquiry, ct).AsTask();
    }
}

public sealed class MessageRepository(AppDbContext dbContext) : IMessageRepository
{
    public Task AddAsync(Message message, CancellationToken ct = default)
    {
        return dbContext.Messages.AddAsync(message, ct).AsTask();
    }
}

public sealed class ComparisonRepository(AppDbContext dbContext) : IComparisonRepository
{
    public Task<Comparison?> GetByUserIdAsync(string userId, CancellationToken ct = default)
    {
        return dbContext.Comparisons.Include(x => x.Items).FirstOrDefaultAsync(x => x.UserId == userId, ct);
    }

    public Task AddAsync(Comparison comparison, CancellationToken ct = default)
    {
        return dbContext.Comparisons.AddAsync(comparison, ct).AsTask();
    }
}

public sealed class ValuationRepository(AppDbContext dbContext) : IValuationRepository
{
    public Task AddAsync(Valuation valuation, CancellationToken ct = default)
    {
        return dbContext.Valuations.AddAsync(valuation, ct).AsTask();
    }
}

public sealed class FeatureCatalogRepository(AppDbContext dbContext) : IFeatureCatalogRepository
{
    public Task<FeatureCatalogEntry?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return dbContext.FeatureCatalog.FirstOrDefaultAsync(x => x.Code == code, ct);
    }

    public async Task<IReadOnlyList<FeatureCatalogEntry>> ListAsync(CancellationToken ct = default)
    {
        return await dbContext.FeatureCatalog.OrderBy(x => x.Code).ToListAsync(ct);
    }
}

public sealed class DictionaryRepository<T>(AppDbContext dbContext) : IDictionaryRepository<T>
    where T : Entity<int>
{
    public async Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default)
    {
        return await dbContext.Set<T>().ToListAsync(ct);
    }

    public Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return dbContext.Set<T>().FindAsync([id], ct).AsTask();
    }
}

public sealed class ModerationRepository(AppDbContext dbContext) : IModerationRepository
{
    public Task AddAsync(ModerationAction action, CancellationToken ct = default)
    {
        return dbContext.ModerationActions.AddAsync(action, ct).AsTask();
    }
}

public sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return dbContext.SaveChangesAsync(ct);
    }
}