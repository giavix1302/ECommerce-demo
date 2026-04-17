using Domain.Enums;

namespace Application.Common.DTOs;

public record PromotionConditionDto(
    long Id,
    PromotionConditionType ConditionType,
    long? TargetId,
    decimal? Value
);

public record PromotionActionDto(
    long Id,
    PromotionActionType ActionType,
    decimal? DiscountValue,
    long? GiftProductId,
    int? GiftQuantity
);
