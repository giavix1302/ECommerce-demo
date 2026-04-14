namespace Domain.Entities;

public class Cart
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long? CouponId { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Coupon? Coupon { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
