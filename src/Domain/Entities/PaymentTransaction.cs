using Domain.Enums;

namespace Domain.Entities;

public class PaymentTransaction
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long PayOSOrderCode { get; set; }
    public string? PaymentLinkId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? AccountNumber { get; set; }
    public string? Reference { get; set; }
    public DateTime? TransactionDateTime { get; set; }
    public string? Currency { get; set; }
    public string? CounterAccountBankName { get; set; }
    public string? CounterAccountName { get; set; }
    public string? CounterAccountNumber { get; set; }
    public PaymentTransactionStatus Status { get; set; } = PaymentTransactionStatus.PENDING;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Order Order { get; set; } = null!;
}
