using ecommerceApiDemo.Domain.Entities;
using ecommerceApiDemo.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ecommerceApiDemo.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).UseIdentityColumn();

        builder.Property(o => o.Subtotal)
            .IsRequired()
            .HasColumnType("decimal(15,2)");

        builder.Property(o => o.DiscountAmount)
            .HasColumnType("decimal(15,2)")
            .HasDefaultValue(0);

        builder.Property(o => o.RankDiscountAmount)
            .HasColumnType("decimal(15,2)")
            .HasDefaultValue(0);

        builder.Property(o => o.TotalAmount)
            .IsRequired()
            .HasColumnType("decimal(15,2)");

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(OrderStatus.PENDING);

        builder.Property(o => o.ShippingAddress)
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(o => o.PointsEarned).HasDefaultValue(0);
        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.UpdatedAt).IsRequired();

        // Relationships
        builder.HasOne(o => o.Coupon)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CouponId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Reviews)
            .WithOne(r => r.Order)
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
