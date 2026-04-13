using ecommerceApiDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ecommerceApiDemo.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).UseIdentityColumn();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Description).HasColumnType("text");

        builder.Property(p => p.Price)
            .IsRequired()
            .HasColumnType("decimal(15,2)");

        builder.Property(p => p.StockQuantity).HasDefaultValue(0);

        builder.Property(p => p.Sku).HasMaxLength(100);
        builder.HasIndex(p => p.Sku).IsUnique().HasFilter("[sku] IS NOT NULL");

        builder.Property(p => p.IsDeleted).HasDefaultValue(false);
        builder.Property(p => p.DeletedAt).IsRequired(false);

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();
    }
}
