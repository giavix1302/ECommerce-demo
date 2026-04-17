using Application.Common.Models;
using Domain.Enums;
using MediatR;

namespace Application.Features.Promotion.Queries.GetPromotions;

public record GetPromotionsQuery(
    int Page = 1,
    int PageSize = 10
) : IRequest<PagedResult<PromotionDto>>;

public record PromotionDto(
    long Id,
    string Name,
    PromotionType Type,
    int Priority,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    bool AllowCoupon
);
