using Domain.Enums;
using MediatR;

namespace Application.Features.Coupon.Commands.CreateCoupon;

public record CreateCouponCommand(
    string Code,
    DiscountType DiscountType,
    decimal Value,
    decimal MinOrderValue,
    DateTime StartDate,
    DateTime EndDate,
    int? UsageLimit
) : IRequest<CreateCouponResult>;

public record CreateCouponResult(long Id, string Code);