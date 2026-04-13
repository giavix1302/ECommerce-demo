using ecommerceApiDemo.Domain.Enums;

namespace ecommerceApiDemo.Domain.Entities;

public class Order
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long? CouponId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal RankDiscountAmount { get; set; } = 0;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.PENDING;
    public string? ShippingAddress { get; set; }
    public int PointsEarned { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Coupon? Coupon { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
