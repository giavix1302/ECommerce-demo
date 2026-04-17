
using Application.Common.Interfaces;
using Domain.Entities;

namespace Application.Common.Services;

public class PromotionEngineService
{
    private readonly IUnitOfWork _unitOfWork;

    public PromotionEngineService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Implement promotion evaluation and application logic here
    public async Task<(PromotionRule?, decimal)> EvaluateAsync(Cart cart)
    {
        var rules = await _unitOfWork.Promotions.GetActivePromotionRulesWithDetailsAsync();

        PromotionRule? bestRule = null;
        decimal bestDiscount = decimal.MinValue;
        foreach (var rule in rules)
        {
            if (!EvaluateConditions(rule, cart)) continue;

            var discount = CalculateDiscount(rule, cart);

            if (bestRule == null)
            {
                bestRule = rule;
                bestDiscount = discount;
                continue;
            }

            // Ưu tiên discount lớn hơn
            if (discount > bestDiscount)
            {
                bestRule = rule;
                bestDiscount = discount;
            }
            // Nếu discount bằng nhau → so priority
            else if (discount == bestDiscount && rule.Priority > bestRule.Priority)
            {
                bestRule = rule;
            }
        }

        return (bestRule, bestDiscount == decimal.MinValue ? 0 : bestDiscount);
    }

    private bool EvaluateConditions(PromotionRule rule, Cart cart)
    {
        // Collect target ProductIds and CategoryIds from rule conditions (used for MIN_QUANTITY scoping)
        var targetProductIds = rule.Conditions
            .Where(c => c.ConditionType == Domain.Enums.PromotionConditionType.PRODUCT)
            .Select(c => c.TargetId)
            .ToHashSet();

        var targetCategoryIds = rule.Conditions
            .Where(c => c.ConditionType == Domain.Enums.PromotionConditionType.CATEGORY)
            .Select(c => c.TargetId)
            .ToHashSet();

        // All conditions must pass (AND logic)
        return rule.Conditions.All(condition =>
        {
            switch (condition.ConditionType)
            {
                case Domain.Enums.PromotionConditionType.MIN_ORDER_VALUE:
                    var cartTotal = cart.CartItems.Sum(ci => ci.Quantity * ci.UnitPrice);
                    return cartTotal >= condition.Value;

                case Domain.Enums.PromotionConditionType.MIN_QUANTITY:
                    // If no PRODUCT/CATEGORY targets defined, fall back to counting all cart items
                    var hasTargets = targetProductIds.Count > 0 || targetCategoryIds.Count > 0;
                    var relevantQuantity = hasTargets
                        ? cart.CartItems
                            .Where(ci =>
                                targetProductIds.Contains(ci.Variant.ProductId) ||
                                targetCategoryIds.Contains(ci.Variant.Product.CategoryId))
                            .Sum(ci => ci.Quantity)
                        : cart.CartItems.Sum(ci => ci.Quantity);
                    return relevantQuantity >= condition.Value;

                case Domain.Enums.PromotionConditionType.PRODUCT:
                    return cart.CartItems.Any(ci => ci.Variant.ProductId == condition.TargetId);

                case Domain.Enums.PromotionConditionType.CATEGORY:
                    return cart.CartItems.Any(ci => ci.Variant.Product.CategoryId == condition.TargetId);

                default:
                    return false;
            }
        });
    }

    private decimal CalculateDiscount(PromotionRule rule, Cart cart)
    {
        // lấy action đầu tiên của rule
        // switch trên action.ActionType
        // ...
        var action = rule.Actions.FirstOrDefault();
        if (action == null) return 0;

        var subtotal = cart.CartItems.Sum(ci => ci.Quantity * ci.UnitPrice);

        return action.ActionType switch
        {
            Domain.Enums.PromotionActionType.PERCENTAGE_DISCOUNT =>
                subtotal * (action.DiscountValue ?? 0) / 100,

            Domain.Enums.PromotionActionType.FIXED_DISCOUNT =>
                action.DiscountValue ?? 0,

            Domain.Enums.PromotionActionType.GIVE_GIFT =>
                (action.GiftProduct?.Variants.FirstOrDefault(v => v.IsDefault)?.Price ?? 0)
                * (action.GiftQuantity ?? 0),

            _ => 0
        };
    }
}