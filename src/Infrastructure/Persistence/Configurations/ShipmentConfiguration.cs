using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("shipments");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).UseIdentityColumn();

        builder.Property(s => s.AhamoveOrderId)
            .IsRequired()
            .HasMaxLength(255);
        builder.HasIndex(s => s.AhamoveOrderId).IsUnique();

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.ServiceId).HasMaxLength(50).IsRequired(false);

        builder.Property(s => s.TotalFee)
            .IsRequired()
            .HasColumnType("decimal(15,2)");

        builder.Property(s => s.Distance)
            .HasColumnType("decimal(10,2)")
            .IsRequired(false);

        builder.Property(s => s.CodAmount)
            .HasColumnType("decimal(15,2)")
            .HasDefaultValue(0);

        builder.Property(s => s.SharedLink).HasMaxLength(500).IsRequired(false);
        builder.Property(s => s.PickupAddress).IsRequired().HasColumnType("text");
        builder.Property(s => s.DeliveryAddress).IsRequired().HasColumnType("text");
        builder.Property(s => s.SupplierId).HasMaxLength(100).IsRequired(false);
        builder.Property(s => s.SupplierName).HasMaxLength(255).IsRequired(false);
        builder.Property(s => s.AhamoveCreateTime).IsRequired(false);

        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();
    }
}
