using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).UseIdentityColumn();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.FullName).HasMaxLength(255);
        builder.Property(u => u.Phone).HasMaxLength(20);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(UserRole.USER);

        builder.Property(u => u.MembershipRank)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(MembershipRank.SILVER);

        builder.Property(u => u.LoyaltyPoints).HasDefaultValue(0);
        builder.Property(u => u.IsActive).HasDefaultValue(true);

        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt).IsRequired();

        // Relationships
        builder.HasOne(u => u.Cart)
            .WithOne(c => c.User)
            .HasForeignKey<Cart>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Orders)
            .WithOne(o => o.User)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Reviews)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.CouponUsages)
            .WithOne(cu => cu.User)
            .HasForeignKey(cu => cu.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
