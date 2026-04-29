using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UpperCube.Domain.Entities;
using UpperCube.Infrastructure.Identity;

namespace UpperCube.Infrastructure.Persistence.Configurations;

public sealed class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("Favorites");
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.UserId, x.PropertyId }).IsUnique();
    }
}

public sealed class InquiryConfiguration : IEntityTypeConfiguration<Inquiry>
{
    public void Configure(EntityTypeBuilder<Inquiry> builder)
    {
        builder.ToTable("Inquiries");
        builder.Property(x => x.FromUserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.InitialMessage).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.FromUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.FromUserId);
    }
}

public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");
        builder.Property(x => x.SenderId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Text).HasMaxLength(4000).IsRequired();
        builder.HasOne(x => x.Inquiry).WithMany(x => x.Messages).HasForeignKey(x => x.InquiryId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.SenderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.InquiryId, x.SentAt });
    }
}