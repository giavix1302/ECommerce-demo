using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("product_variants");

        builder.HasKey(pv => pv.Id);
        builder.Property(pv => pv.Id).UseIdentityColumn();

        builder.Property(pv => pv.Sku).HasMaxLength(100).IsRequired(false);
        builder.HasIndex(pv => pv.Sku).IsUnique().HasFilter("[sku] IS NOT NULL");
        builder.HasIndex(pv => new { pv.ProductId, pv.IsDeleted });

        builder.Property(pv => pv.Price)
            .IsRequired()
            .HasColumnType("decimal(15,2)");

        builder.Property(pv => pv.StockQuantity).HasDefaultValue(0);
        builder.Property(pv => pv.IsDefault).HasDefaultValue(false);

        builder.Property(pv => pv.IsDeleted).HasDefaultValue(false);
        builder.Property(pv => pv.DeletedAt).IsRequired(false);

        builder.Property(pv => pv.CreatedAt).IsRequired();
        builder.Property(pv => pv.UpdatedAt).IsRequired();

        // Relationships
        builder.HasOne(pv => pv.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(pv => pv.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(pv => pv.AttributeValues)
            .WithOne(av => av.Variant)
            .HasForeignKey(av => av.VariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
