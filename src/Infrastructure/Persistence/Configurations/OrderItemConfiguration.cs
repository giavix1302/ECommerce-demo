using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");

        builder.HasKey(oi => oi.Id);
        builder.Property(oi => oi.Id).UseIdentityColumn();

        builder.Property(oi => oi.Quantity).IsRequired();

        builder.Property(oi => oi.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(15,2)");

        builder.Property(oi => oi.TotalPrice)
            .IsRequired()
            .HasColumnType("decimal(15,2)");

        builder.Property(oi => oi.CreatedAt).IsRequired();

        builder.HasIndex(oi => oi.OrderId);

        // Relationships
        builder.HasOne(oi => oi.Variant)
            .WithMany(v => v.OrderItems)
            .HasForeignKey(oi => oi.VariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(oi => oi.PromotionRule)
            .WithMany(pr => pr.OrderItems)
            .HasForeignKey(oi => oi.PromotionRuleId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
