using Domain.Enums;

namespace Domain.Entities;

public class User
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public UserRole Role { get; set; } = UserRole.USER;
    public MembershipRank MembershipRank { get; set; } = MembershipRank.SILVER;
    public int LoyaltyPoints { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Cart? Cart { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();
}
