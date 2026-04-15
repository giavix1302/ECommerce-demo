using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder.ToTable("product_attributes");

        builder.HasKey(pa => pa.Id);
        builder.Property(pa => pa.Id).UseIdentityColumn();

        builder.Property(pa => pa.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.HasIndex(pa => pa.Name).IsUnique();
    }
}
