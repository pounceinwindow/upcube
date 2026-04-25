using Microsoft.EntityFrameworkCore;
using UpperCube.Application.Abstractions.Repositories;
using UpperCube.Domain.Common;
using UpperCube.Domain.Entities;

namespace UpperCube.Infrastructure.Persistence.Repositories;

public sealed class FavoriteRepository(AppDbContext dbContext) : IFavoriteRepository
{
    public Task<Favorite?> GetAsync(string userId, int propertyId, CancellationToken ct = default) =>
        dbContext.Favorites.FirstOrDefaultAsync(x => x.UserId == userId && x.PropertyId == propertyId, ct);

    public Task AddAsync(Favorite favorite, CancellationToken ct = default) =>
        dbContext.Favorites.AddAsync(favorite, ct).AsTask();

    public Task DeleteAsync(Favorite favorite, CancellationToken ct = default)
    {
        dbContext.Favorites.Remove(favorite);
        return Task.CompletedTask;
    }
}

public sealed class InquiryRepository(AppDbContext dbContext) : IInquiryRepository
{
    public Task<Inquiry?> GetByIdAsync(int id, CancellationToken ct = default) =>
        dbContext.Inquiries.Include(x => x.Messages).FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task AddAsync(Inquiry inquiry, CancellationToken ct = default) =>
        dbContext.Inquiries.AddAsync(inquiry, ct).AsTask();
}

public sealed class MessageRepository(AppDbContext dbContext) : IMessageRepository
{
    public Task AddAsync(Message message, CancellationToken ct = default) =>
        dbContext.Messages.AddAsync(message, ct).AsTask();
}

public sealed class ComparisonRepository(AppDbContext dbContext) : IComparisonRepository
{
    public Task<Comparison?> GetByUserIdAsync(string userId, CancellationToken ct = default) =>
        dbContext.Comparisons.Include(x => x.Items).FirstOrDefaultAsync(x => x.UserId == userId, ct);

    public Task AddAsync(Comparison comparison, CancellationToken ct = default) =>
        dbContext.Comparisons.AddAsync(comparison, ct).AsTask();
}

public sealed class ValuationRepository(AppDbContext dbContext) : IValuationRepository
{
    public Task AddAsync(Valuation valuation, CancellationToken ct = default) =>
        dbContext.Valuations.AddAsync(valuation, ct).AsTask();
}

public sealed class FeatureCatalogRepository(AppDbContext dbContext) : IFeatureCatalogRepository
{
    public Task<FeatureCatalogEntry?> GetByCodeAsync(string code, CancellationToken ct = default) =>
        dbContext.FeatureCatalog.FirstOrDefaultAsync(x => x.Code == code, ct);

    public async Task<IReadOnlyList<FeatureCatalogEntry>> ListAsync(CancellationToken ct = default) =>
        await dbContext.FeatureCatalog.OrderBy(x => x.Code).ToListAsync(ct);
}

public sealed class DictionaryRepository<T>(AppDbContext dbContext) : IDictionaryRepository<T>
    where T : Entity
{
    public async Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default) =>
        await dbContext.Set<T>().ToListAsync(ct);

    public Task<T?> GetByIdAsync(int id, CancellationToken ct = default) =>
        dbContext.Set<T>().FindAsync([id], ct).AsTask();
}

public sealed class ModerationRepository(AppDbContext dbContext) : IModerationRepository
{
    public Task AddAsync(ModerationAction action, CancellationToken ct = default) =>
        dbContext.ModerationActions.AddAsync(action, ct).AsTask();
}

public sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        dbContext.SaveChangesAsync(ct);
}
