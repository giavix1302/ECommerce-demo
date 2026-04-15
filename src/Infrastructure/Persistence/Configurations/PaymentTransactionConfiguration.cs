using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("payment_transactions");

        builder.HasKey(pt => pt.Id);
        builder.Property(pt => pt.Id).UseIdentityColumn();

        builder.HasIndex(pt => pt.PayOSOrderCode).IsUnique();

        builder.Property(pt => pt.PaymentLinkId).HasMaxLength(255).IsRequired(false);

        builder.Property(pt => pt.Amount)
            .IsRequired()
            .HasColumnType("decimal(15,2)");

        builder.Property(pt => pt.Description).HasColumnType("text").IsRequired(false);
        builder.Property(pt => pt.AccountNumber).HasMaxLength(50).IsRequired(false);
        builder.Property(pt => pt.Reference).HasMaxLength(255).IsRequired(false);
        builder.Property(pt => pt.TransactionDateTime).IsRequired(false);
        builder.Property(pt => pt.Currency).HasMaxLength(10).IsRequired(false);
        builder.Property(pt => pt.CounterAccountBankName).HasMaxLength(255).IsRequired(false);
        builder.Property(pt => pt.CounterAccountName).HasMaxLength(255).IsRequired(false);
        builder.Property(pt => pt.CounterAccountNumber).HasMaxLength(50).IsRequired(false);

        builder.Property(pt => pt.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(pt => pt.CreatedAt).IsRequired();
        builder.Property(pt => pt.UpdatedAt).IsRequired();
    }
}
