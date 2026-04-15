using Domain.Enums;

namespace Domain.Entities;

public class PromotionAction
{
    public long Id { get; set; }
    public long PromotionRuleId { get; set; }
    public PromotionActionType ActionType { get; set; }
    public decimal? DiscountValue { get; set; }
    public long? GiftProductId { get; set; }
    public int? GiftQuantity { get; set; }

    // Navigation properties
    public PromotionRule PromotionRule { get; set; } = null!;
    public Product? GiftProduct { get; set; }
}
