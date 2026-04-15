using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class VariantAttributeValueConfiguration : IEntityTypeConfiguration<VariantAttributeValue>
{
    public void Configure(EntityTypeBuilder<VariantAttributeValue> builder)
    {
        builder.ToTable("variant_attribute_values");

        builder.HasKey(vav => vav.Id);
        builder.Property(vav => vav.Id).UseIdentityColumn();

        builder.Property(vav => vav.Value)
            .IsRequired()
            .HasMaxLength(255);

        // Relationships
        builder.HasOne(vav => vav.Attribute)
            .WithMany(pa => pa.VariantAttributeValues)
            .HasForeignKey(vav => vav.AttributeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
