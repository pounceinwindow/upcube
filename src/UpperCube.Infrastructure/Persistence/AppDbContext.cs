using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UpperCube.Domain.Entities;
using UpperCube.Infrastructure.Identity;

namespace UpperCube.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<City> Cities => Set<City>();

    public DbSet<District> Districts => Set<District>();

    public DbSet<PropertyType> PropertyTypes => Set<PropertyType>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Amenity> Amenities => Set<Amenity>();

    public DbSet<Property> Properties => Set<Property>();

    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();

    public DbSet<PropertyAmenity> PropertyAmenities => Set<PropertyAmenity>();

    public DbSet<Favorite> Favorites => Set<Favorite>();

    public DbSet<Inquiry> Inquiries => Set<Inquiry>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<Comparison> Comparisons => Set<Comparison>();

    public DbSet<ComparisonItem> ComparisonItems => Set<ComparisonItem>();

    public DbSet<Valuation> Valuations => Set<Valuation>();

    public DbSet<FeatureCatalogEntry> FeatureCatalog => Set<FeatureCatalogEntry>();

    public DbSet<ModerationAction> ModerationActions => Set<ModerationAction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
