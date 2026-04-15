using Domain.Enums;

namespace Domain.Entities;

public class PromotionCondition
{
    public long Id { get; set; }
    public long PromotionRuleId { get; set; }
    public PromotionConditionType ConditionType { get; set; }
    public long? TargetId { get; set; }
    public decimal? Value { get; set; }

    // Navigation properties
    public PromotionRule PromotionRule { get; set; } = null!;
}
