using Domain.Enums;

namespace Domain.Entities;

public class PromotionRule
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PromotionType Type { get; set; }
    public int Priority { get; set; } = 0;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool AllowCoupon { get; set; } = true;

    // Navigation properties
    public ICollection<PromotionCondition> Conditions { get; set; } = new List<PromotionCondition>();
    public ICollection<PromotionAction> Actions { get; set; } = new List<PromotionAction>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
