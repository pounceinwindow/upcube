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
    public Task<int> CountAsync(CancellationToken ct = default)
    {
        return dbContext.Inquiries.CountAsync(ct);
    }

    public Task<Inquiry?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return dbContext.Inquiries
            .Include(x => x.Property)
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyList<Inquiry>> GetByUserIdAsync(string userId, CancellationToken ct = default)
    {
        return await dbContext.Inquiries
            .Where(x => x.FromUserId == userId)
            .Include(x => x.Property)
            .Include(x => x.Messages)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Inquiry>> GetByAgentIdAsync(string agentId, CancellationToken ct = default)
    {
        return await dbContext.Inquiries
            .Include(x => x.Property)
            .Include(x => x.Messages)
            .Where(x => x.Property != null && x.Property.AgentId == agentId)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(ct);
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
        return dbContext.Comparisons
            .Include(x => x.Items)
            .ThenInclude(x => x.Property)
            .ThenInclude(x => x!.Images)
            .Include(x => x.Items)
            .ThenInclude(x => x.Property)
            .ThenInclude(x => x!.City)
            .Include(x => x.Items)
            .ThenInclude(x => x.Property)
            .ThenInclude(x => x!.District)
            .Include(x => x.Items)
            .ThenInclude(x => x.Property)
            .ThenInclude(x => x!.PropertyType)
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);
    }

    public async Task<IReadOnlyList<int>> GetUserComparisonPropertyIdsAsync(string userId,
        CancellationToken ct = default)
    {
        return await dbContext.ComparisonItems
            .Where(x => x.Comparison != null && x.Comparison.UserId == userId)
            .Select(x => x.PropertyId)
            .ToListAsync(ct);
    }

    public Task AddAsync(Comparison comparison, CancellationToken ct = default)
    {
        return dbContext.Comparisons.AddAsync(comparison, ct).AsTask();
    }

    public void RemoveItem(ComparisonItem item)
    {
        dbContext.ComparisonItems.Remove(item);
    }

    public void RemoveItems(IEnumerable<ComparisonItem> items)
    {
        dbContext.ComparisonItems.RemoveRange(items);
    }
}

public sealed class ValuationRepository(AppDbContext dbContext) : IValuationRepository
{
    public Task<int> CountAsync(CancellationToken ct = default)
    {
        return dbContext.Valuations.CountAsync(ct);
    }

    public Task AddAsync(Valuation valuation, CancellationToken ct = default)
    {
        return dbContext.Valuations.AddAsync(valuation, ct).AsTask();
    }
}

public sealed class FeatureCatalogRepository(AppDbContext dbContext) : IFeatureCatalogRepository
{
    public Task<int> CountAsync(CancellationToken ct = default)
    {
        return dbContext.FeatureCatalog.CountAsync(ct);
    }

    public Task<FeatureCatalogEntry?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return dbContext.FeatureCatalog.FirstOrDefaultAsync(x => x.Code == code, ct);
    }

    public async Task<IReadOnlyList<FeatureCatalogEntry>> ListAsync(CancellationToken ct = default)
    {
        return await dbContext.FeatureCatalog.OrderBy(x => x.Code).ToListAsync(ct);
    }

    public Task UpdateAsync(FeatureCatalogEntry entry, CancellationToken ct = default)
    {
        if (dbContext.Entry(entry).State == EntityState.Detached) dbContext.FeatureCatalog.Update(entry);
        return Task.CompletedTask;
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
    public async Task<IReadOnlyList<ModerationAction>> GetRecentAsync(int count, CancellationToken ct = default)
    {
        count = Math.Clamp(count, 1, 100);

        return await dbContext.ModerationActions
            .Include(x => x.Property)
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .ToListAsync(ct);
    }

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
