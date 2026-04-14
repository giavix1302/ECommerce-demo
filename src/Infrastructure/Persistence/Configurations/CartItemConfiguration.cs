using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("cart_items");

        builder.HasKey(ci => ci.Id);
        builder.Property(ci => ci.Id).UseIdentityColumn();

        builder.Property(ci => ci.Quantity).IsRequired();

        builder.Property(ci => ci.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(15,2)");

        builder.Property(ci => ci.CreatedAt).IsRequired();

        // Relationships
        builder.HasOne(ci => ci.Product)
            .WithMany(p => p.CartItems)
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
