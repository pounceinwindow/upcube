using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UpperCube.Domain.Entities;
using UpperCube.Infrastructure.Identity;

namespace UpperCube.Infrastructure.Persistence.Configurations;

public sealed class ComparisonConfiguration : IEntityTypeConfiguration<Comparison>
{
    public void Configure(EntityTypeBuilder<Comparison> builder)
    {
        builder.ToTable("Comparisons");
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.UserId);
    }
}

public sealed class ComparisonItemConfiguration : IEntityTypeConfiguration<ComparisonItem>
{
    public void Configure(EntityTypeBuilder<ComparisonItem> builder)
    {
        builder.ToTable("ComparisonItems");
        builder.HasKey(x => new { x.ComparisonId, x.PropertyId });
        builder.HasOne(x => x.Comparison).WithMany(x => x.Items).HasForeignKey(x => x.ComparisonId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.ComparisonId, x.Position }).IsUnique();
    }
}

public sealed class ValuationConfiguration : IEntityTypeConfiguration<Valuation>
{
    public void Configure(EntityTypeBuilder<Valuation> builder)
    {
        builder.ToTable("Valuations");
        builder.Property(x => x.UserId).HasMaxLength(450);
        builder.Property(x => x.InputJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.EstimatedMin).HasPrecision(18, 2);
        builder.Property(x => x.EstimatedMax).HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.StrategyUsed).HasMaxLength(128).IsRequired();
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => x.UserId);
    }
}

public sealed class FeatureCatalogEntryConfiguration : IEntityTypeConfiguration<FeatureCatalogEntry>
{
    public void Configure(EntityTypeBuilder<FeatureCatalogEntry> builder)
    {
        builder.ToTable("FeatureCatalog");
        builder.Property(x => x.Code).HasMaxLength(128).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(180).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
    }
}

public sealed class ModerationActionConfiguration : IEntityTypeConfiguration<ModerationAction>
{
    public void Configure(EntityTypeBuilder<ModerationAction> builder)
    {
        builder.ToTable("ModerationActions");
        builder.Property(x => x.ModeratorId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Action).HasConversion<int>();
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ModeratorId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.ModeratorId);
    }
}