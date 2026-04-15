using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PromotionConditionConfiguration : IEntityTypeConfiguration<PromotionCondition>
{
    public void Configure(EntityTypeBuilder<PromotionCondition> builder)
    {
        builder.ToTable("promotion_conditions");

        builder.HasKey(pc => pc.Id);
        builder.Property(pc => pc.Id).UseIdentityColumn();

        builder.Property(pc => pc.ConditionType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(pc => pc.TargetId).IsRequired(false);

        builder.Property(pc => pc.Value)
            .HasColumnType("decimal(15,2)")
            .IsRequired(false);
    }
}
