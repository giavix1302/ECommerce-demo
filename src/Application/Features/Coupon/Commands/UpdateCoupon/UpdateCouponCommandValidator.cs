using FluentValidation;

namespace Application.Features.Coupon.Commands.UpdateCoupon;

public class UpdateCouponCommandValidator : AbstractValidator<UpdateCouponCommand>
{
    public UpdateCouponCommandValidator()
    {
        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("Discount value must be greater than zero.");

        RuleFor(x => x.MinOrderValue)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum order value must be greater than or equal to zero.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0).When(x => x.UsageLimit.HasValue).WithMessage("Usage limit must be greater than zero if specified.");
    }
}
