using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UpperCube.Domain.Entities;

namespace UpperCube.Infrastructure.Persistence.Configurations;

public sealed class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("Cities");
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(128).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
    }
}

public sealed class DistrictConfiguration : IEntityTypeConfiguration<District>
{
    public void Configure(EntityTypeBuilder<District> builder)
    {
        builder.ToTable("Districts");
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(128).IsRequired();
        builder.HasIndex(x => new { x.CityId, x.Slug }).IsUnique();
        builder.HasOne(x => x.City).WithMany(x => x.Districts).HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class PropertyTypeConfiguration : IEntityTypeConfiguration<PropertyType>
{
    public void Configure(EntityTypeBuilder<PropertyType> builder)
    {
        builder.ToTable("PropertyTypes");
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(128).IsRequired();
        builder.Property(x => x.IconClass).HasMaxLength(128).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
    }
}

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(128).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
    }
}

public sealed class AmenityConfiguration : IEntityTypeConfiguration<Amenity>
{
    public void Configure(EntityTypeBuilder<Amenity> builder)
    {
        builder.ToTable("Amenities");
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.IconClass).HasMaxLength(128).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
    }
}