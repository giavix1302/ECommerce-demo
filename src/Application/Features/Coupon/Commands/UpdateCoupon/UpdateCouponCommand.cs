using MediatR;

namespace Application.Features.Coupon.Commands.UpdateCoupon;

public record UpdateCouponCommand(
    decimal Value,
    decimal MinOrderValue,
    DateTime StartDate,
    DateTime EndDate,
    int? UsageLimit,
    bool IsActive
) : IRequest
{
    public long Id { get; init; }
}
