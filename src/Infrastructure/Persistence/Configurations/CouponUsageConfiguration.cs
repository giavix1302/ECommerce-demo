using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
{
    public void Configure(EntityTypeBuilder<CouponUsage> builder)
    {
        builder.ToTable("coupon_usages");

        builder.HasKey(cu => cu.Id);
        builder.Property(cu => cu.Id).UseIdentityColumn();

        builder.Property(cu => cu.UsedAt).IsRequired();

        // Relationships
        builder.HasOne(cu => cu.Coupon)
            .WithMany(c => c.CouponUsages)
            .HasForeignKey(cu => cu.CouponId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cu => cu.Order)
            .WithMany(o => o.CouponUsages)
            .HasForeignKey(cu => cu.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
