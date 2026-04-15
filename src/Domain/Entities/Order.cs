using Domain.Enums;

namespace Domain.Entities;

public class Order
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long? CouponId { get; set; }
    public long? PromotionRuleId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal PromotionDiscountAmount { get; set; } = 0;
    public decimal RankDiscountAmount { get; set; } = 0;
    public decimal ShippingFee { get; set; } = 0;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.PENDING;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.UNPAID;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;
    public DateTime? PaymentExpiredAt { get; set; }
    public string? ShippingAddress { get; set; }
    public int PointsEarned { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Coupon? Coupon { get; set; }
    public PromotionRule? PromotionRule { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public Shipment? Shipment { get; set; }
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
}
