using Domain.Enums;
using MediatR;

namespace Application.Features.Order.Queries.GetCheckoutPreview;

public record GetCheckoutPreviewQuery : IRequest<CheckoutPreviewDto>;

public record CheckoutPreviewDto(
    IEnumerable<CheckoutItemDto> Items,
    decimal Subtotal,
    CheckoutPromotionDto? Promotion,
    CheckoutCouponDto? Coupon,
    MembershipRank MembershipRank,
    decimal RankDiscountAmount,
    decimal? ShippingFee,
    decimal TotalAmount
);

public record CheckoutItemDto(
    long CartItemId,
    long VariantId,
    string ProductName,
    string? Sku,
    IEnumerable<CheckoutItemAttributeDto> Attributes,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice
);

public record CheckoutItemAttributeDto(
    string AttributeName,
    string Value
);

// ── Promotion ─────────────────────────────────────────────────────────────────

public record CheckoutPromotionDto(
    long PromotionRuleId,
    string Name,
    PromotionType Type,
    decimal PromotionDiscount,
    bool AllowCoupon,
    CheckoutGiftDto? Gift             // non-null only when Type = BUY_X_GET_Y + GIVE_GIFT
);

public record CheckoutGiftDto(
    long GiftProductId,
    string GiftProductName,
    int GiftQuantity,
    IEnumerable<CheckoutGiftVariantDto> AvailableVariants
);

public record CheckoutGiftVariantDto(
    long VariantId,
    string? Sku,
    IEnumerable<CheckoutItemAttributeDto> Attributes,
    decimal Price
);

// ── Coupon ────────────────────────────────────────────────────────────────────

public record CheckoutCouponDto(
    string Code,
    decimal DiscountAmount            // recalculated on Base if Promotion exists
);
