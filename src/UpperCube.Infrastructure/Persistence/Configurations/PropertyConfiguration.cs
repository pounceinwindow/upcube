using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UpperCube.Domain.Entities;
using UpperCube.Infrastructure.Identity;

namespace UpperCube.Infrastructure.Persistence.Configurations;

public sealed class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("Properties");
        builder.Property(x => x.Title).HasMaxLength(180).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(5000).IsRequired();
        builder.Property(x => x.AgentId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Address).HasMaxLength(256).IsRequired();
        builder.Property(x => x.TransactionType).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();

        builder.OwnsOne(x => x.Price, price =>
        {
            price.Property(x => x.Amount).HasColumnName("Price_Amount").HasPrecision(18, 2);
            price.Property(x => x.Currency).HasColumnName("Price_Currency").HasMaxLength(3).IsRequired();
        });

        builder.OwnsOne(x => x.Area, area =>
        {
            area.Property(x => x.Value).HasColumnName("Area_Value").HasPrecision(12, 2);
            area.Property(x => x.Unit).HasColumnName("Area_Unit").HasConversion<int>();
        });

        builder.OwnsOne(x => x.Location, location =>
        {
            location.Property(x => x.Latitude).HasColumnName("Location_Latitude");
            location.Property(x => x.Longitude).HasColumnName("Location_Longitude");
        });

        builder.HasOne(x => x.City).WithMany().HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.District).WithMany().HasForeignKey(x => x.DistrictId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PropertyType).WithMany().HasForeignKey(x => x.PropertyTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.AgentId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.AgentId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.CityId, x.DistrictId });
        builder.HasIndex(x => x.PropertyTypeId);
        builder.HasIndex(x => x.CategoryId);
    }
}

public sealed class PropertyImageConfiguration : IEntityTypeConfiguration<PropertyImage>
{
    public void Configure(EntityTypeBuilder<PropertyImage> builder)
    {
        builder.ToTable("PropertyImages");
        builder.Property(x => x.Path).HasMaxLength(512).IsRequired();
        builder.HasOne(x => x.Property).WithMany(x => x.Images).HasForeignKey(x => x.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.PropertyId, x.Order });
    }
}

public sealed class PropertyAmenityConfiguration : IEntityTypeConfiguration<PropertyAmenity>
{
    public void Configure(EntityTypeBuilder<PropertyAmenity> builder)
    {
        builder.ToTable("PropertyAmenities");
        builder.HasKey(x => new { x.PropertyId, x.AmenityId });
        builder.HasOne(x => x.Property).WithMany(x => x.Amenities).HasForeignKey(x => x.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Amenity).WithMany().HasForeignKey(x => x.AmenityId).OnDelete(DeleteBehavior.Cascade);
    }
}
