using Application.Common.Models;
using MediatR;

namespace Application.Features.Coupon.Queries.GetCoupons;

public record GetCouponsQuery(
    int Page = 1,
    int PageSize = 10
) : IRequest<PagedResult<CouponDto>>;

public record CouponDto(
    long Id,
    string Code,
    string DiscountType,
    decimal Value,
    decimal MinOrderValue,
    DateTime StartDate,
    DateTime EndDate,
    int? UsageLimit,
    int UsedCount,
    bool IsActive
);