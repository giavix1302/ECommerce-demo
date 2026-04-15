using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PromotionRuleConfiguration : IEntityTypeConfiguration<PromotionRule>
{
    public void Configure(EntityTypeBuilder<PromotionRule> builder)
    {
        builder.ToTable("promotion_rules");

        builder.HasKey(pr => pr.Id);
        builder.Property(pr => pr.Id).UseIdentityColumn();

        builder.Property(pr => pr.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(pr => pr.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(pr => pr.Priority).HasDefaultValue(0);
        builder.Property(pr => pr.StartDate).IsRequired();
        builder.Property(pr => pr.EndDate).IsRequired();
        builder.Property(pr => pr.IsActive).HasDefaultValue(true);
        builder.Property(pr => pr.AllowCoupon).HasDefaultValue(true);

        // Relationships
        builder.HasMany(pr => pr.Conditions)
            .WithOne(c => c.PromotionRule)
            .HasForeignKey(c => c.PromotionRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(pr => pr.Actions)
            .WithOne(a => a.PromotionRule)
            .HasForeignKey(a => a.PromotionRuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
