using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

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

        builder.Property(o => o.PromotionDiscountAmount)
            .HasColumnType("decimal(15,2)")
            .HasDefaultValue(0);

        builder.Property(o => o.RankDiscountAmount)
            .HasColumnType("decimal(15,2)")
            .HasDefaultValue(0);

        builder.Property(o => o.ShippingFee)
            .HasColumnType("decimal(15,2)")
            .HasDefaultValue(0);

        builder.Property(o => o.TotalAmount)
            .IsRequired()
            .HasColumnType("decimal(15,2)");

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(OrderStatus.PENDING);

        builder.Property(o => o.PaymentStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(PaymentStatus.UNPAID);

        builder.Property(o => o.PaymentMethod)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(PaymentMethod.COD);

        builder.Property(o => o.PaymentExpiredAt).IsRequired(false);

        builder.Property(o => o.ShippingAddress)
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(o => o.PointsEarned).HasDefaultValue(0);
        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.UpdatedAt).IsRequired();

        builder.HasIndex(o => new { o.UserId, o.Status });

        // Relationships
        builder.HasOne(o => o.Coupon)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CouponId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasOne(o => o.PromotionRule)
            .WithMany(pr => pr.Orders)
            .HasForeignKey(o => o.PromotionRuleId)
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

        builder.HasOne(o => o.Shipment)
            .WithOne(s => s.Order)
            .HasForeignKey<Shipment>(s => s.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.PaymentTransactions)
            .WithOne(pt => pt.Order)
            .HasForeignKey(pt => pt.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
