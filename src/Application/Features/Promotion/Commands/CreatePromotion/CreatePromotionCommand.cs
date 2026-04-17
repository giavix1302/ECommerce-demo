using Application.Common.DTOs;
using Domain.Enums;
using MediatR;

namespace Application.Features.Promotion.Commands.CreatePromotion;

public record CreatePromotionCommand(
    string Name,
    PromotionType Type,
    int Priority,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    bool AllowCoupon,
    IEnumerable<CreatePromotionConditionDto> Conditions,
    IEnumerable<CreatePromotionActionDto> Actions
) : IRequest<CreatePromotionResult>;

public record CreatePromotionConditionDto(
    PromotionConditionType ConditionType,
    long? TargetId,
    decimal? Value
);

public record CreatePromotionActionDto(
    PromotionActionType ActionType,
    decimal? DiscountValue,
    long? GiftProductId,
    int? GiftQuantity
);

public record CreatePromotionResult(
    long Id,
    string Name,
    PromotionType Type,
    int Priority,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    bool AllowCoupon,
    IEnumerable<PromotionConditionDto> Conditions,
    IEnumerable<PromotionActionDto> Actions
);