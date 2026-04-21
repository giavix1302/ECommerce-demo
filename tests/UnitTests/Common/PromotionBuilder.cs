using Domain.Entities;
using Domain.Enums;

namespace UnitTests.Common;

internal class PromotionBuilder
{
    private readonly PromotionRule _rule;

    public PromotionBuilder(long id = 1, string name = "Test Promo", int priority = 0)
    {
        _rule = new PromotionRule
        {
            Id = id,
            Name = name,
            Type = PromotionType.CUSTOM_DISCOUNT,
            Priority = priority,
            IsActive = true,
            AllowCoupon = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1)
        };
    }

    public PromotionBuilder WithMinOrderValue(decimal minValue)
    {
        _rule.Conditions.Add(new PromotionCondition
        {
            ConditionType = PromotionConditionType.MIN_ORDER_VALUE,
            Value = minValue
        });
        return this;
    }

    public PromotionBuilder WithMinQuantity(decimal qty, long? productId = null, long? categoryId = null)
    {
        if (productId.HasValue)
            _rule.Conditions.Add(new PromotionCondition
            {
                ConditionType = PromotionConditionType.PRODUCT,
                TargetId = productId.Value
            });
        if (categoryId.HasValue)
            _rule.Conditions.Add(new PromotionCondition
            {
                ConditionType = PromotionConditionType.CATEGORY,
                TargetId = categoryId.Value
            });
        _rule.Conditions.Add(new PromotionCondition
        {
            ConditionType = PromotionConditionType.MIN_QUANTITY,
            Value = qty
        });
        return this;
    }

    public PromotionBuilder WithPercentageDiscount(decimal pct)
    {
        _rule.Actions.Add(new PromotionAction
        {
            ActionType = PromotionActionType.PERCENTAGE_DISCOUNT,
            DiscountValue = pct
        });
        return this;
    }

    public PromotionBuilder WithFixedDiscount(decimal amount)
    {
        _rule.Actions.Add(new PromotionAction
        {
            ActionType = PromotionActionType.FIXED_DISCOUNT,
            DiscountValue = amount
        });
        return this;
    }

    public PromotionBuilder WithGiftAction(long giftProductId, int giftQty = 1, decimal giftPrice = 50_000)
    {
        _rule.Type = PromotionType.BUY_X_GET_Y;
        _rule.Actions.Add(new PromotionAction
        {
            ActionType = PromotionActionType.GIVE_GIFT,
            GiftProductId = giftProductId,
            GiftQuantity = giftQty,
            GiftProduct = new Product
            {
                Id = giftProductId,
                Variants = [new ProductVariant { IsDefault = true, Price = giftPrice }]
            }
        });
        return this;
    }

    public PromotionBuilder DisallowCoupon()
    {
        _rule.AllowCoupon = false;
        return this;
    }

    public PromotionRule Build() => _rule;
}
