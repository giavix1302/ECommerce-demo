using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("coupons");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).UseIdentityColumn();

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(100);
        builder.HasIndex(c => c.Code).IsUnique();

        builder.Property(c => c.DiscountType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.Value)
            .IsRequired()
            .HasColumnType("decimal(15,2)");

        builder.Property(c => c.MinOrderValue)
            .HasColumnType("decimal(15,2)")
            .HasDefaultValue(0);

        builder.Property(c => c.StartDate).IsRequired();
        builder.Property(c => c.EndDate).IsRequired();

        builder.Property(c => c.UsageLimit).IsRequired(false);
        builder.Property(c => c.UsedCount).HasDefaultValue(0);
        builder.Property(c => c.IsActive).HasDefaultValue(true);
        builder.Property(c => c.CreatedAt).IsRequired();
    }
}
