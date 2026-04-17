using Application.Common.DTOs;
using Domain.Enums;
using MediatR;

namespace Application.Features.Promotion.Queries.GetPromotionById;

public record GetPromotionByIdQuery(long Id) : IRequest<PromotionDetailDto>;

public record PromotionDetailDto(
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
