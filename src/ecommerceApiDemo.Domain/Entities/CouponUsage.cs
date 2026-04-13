namespace ecommerceApiDemo.Domain.Entities;

public class CouponUsage
{
    public long Id { get; set; }
    public long CouponId { get; set; }
    public long UserId { get; set; }
    public long OrderId { get; set; }
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Coupon Coupon { get; set; } = null!;
    public Order Order { get; set; } = null!;
}
