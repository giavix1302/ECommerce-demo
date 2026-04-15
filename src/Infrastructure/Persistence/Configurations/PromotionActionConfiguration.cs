using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PromotionActionConfiguration : IEntityTypeConfiguration<PromotionAction>
{
    public void Configure(EntityTypeBuilder<PromotionAction> builder)
    {
        builder.ToTable("promotion_actions");

        builder.HasKey(pa => pa.Id);
        builder.Property(pa => pa.Id).UseIdentityColumn();

        builder.Property(pa => pa.ActionType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(pa => pa.DiscountValue)
            .HasColumnType("decimal(15,2)")
            .IsRequired(false);

        builder.Property(pa => pa.GiftProductId).IsRequired(false);
        builder.Property(pa => pa.GiftQuantity).IsRequired(false);

        // Relationships
        builder.HasOne(pa => pa.GiftProduct)
            .WithMany()
            .HasForeignKey(pa => pa.GiftProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
