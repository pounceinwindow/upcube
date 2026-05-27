using UpperCube.Domain.Entities;

namespace UpperCube.Application.Abstractions.Repositories;

public interface IFavoriteRepository
{
    Task<Favorite?> GetAsync(string userId, int propertyId, CancellationToken ct = default);

    Task<IReadOnlyList<int>> GetUserFavoritePropertyIdsAsync(string userId, CancellationToken ct = default);

    Task<IReadOnlyList<Property>> GetUserFavoritesAsync(string userId, CancellationToken ct = default);

    Task AddAsync(Favorite favorite, CancellationToken ct = default);

    Task DeleteAsync(Favorite favorite, CancellationToken ct = default);
}

public interface IInquiryRepository
{
    Task<int> CountAsync(CancellationToken ct = default);

    Task<Inquiry?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<Inquiry>> GetByUserIdAsync(string userId, CancellationToken ct = default);

    Task<IReadOnlyList<Inquiry>> GetByAgentIdAsync(string agentId, CancellationToken ct = default);

    Task AddAsync(Inquiry inquiry, CancellationToken ct = default);
}

public interface IMessageRepository
{
    Task AddAsync(Message message, CancellationToken ct = default);
}

public interface IComparisonRepository
{
    Task<Comparison?> GetByUserIdAsync(string userId, CancellationToken ct = default);

    Task<IReadOnlyList<int>> GetUserComparisonPropertyIdsAsync(string userId, CancellationToken ct = default);

    Task AddAsync(Comparison comparison, CancellationToken ct = default);

    void RemoveItem(ComparisonItem item);

    void RemoveItems(IEnumerable<ComparisonItem> items);
}

public interface IValuationRepository
{
    Task<int> CountAsync(CancellationToken ct = default);

    Task AddAsync(Domain.Entities.Valuation valuation, CancellationToken ct = default);
}

public interface IFeatureCatalogRepository
{
    Task<int> CountAsync(CancellationToken ct = default);

    Task<FeatureCatalogEntry?> GetByCodeAsync(string code, CancellationToken ct = default);

    Task<IReadOnlyList<FeatureCatalogEntry>> ListAsync(CancellationToken ct = default);

    Task UpdateAsync(FeatureCatalogEntry entry, CancellationToken ct = default);
}

public interface IDictionaryRepository<T>
{
    Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default);

    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
}

public interface IModerationRepository
{
    Task<IReadOnlyList<ModerationAction>> GetRecentAsync(int count, CancellationToken ct = default);

    Task AddAsync(ModerationAction action, CancellationToken ct = default);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}