using MediatR;

namespace Application.Features.Coupon.Commands.ApplyCoupon;

public record ApplyCouponCommand(string Code) : IRequest<ApplyCouponResult>;

public record ApplyCouponResult(
    decimal DiscountAmount
);