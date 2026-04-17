using FluentValidation;

namespace Application.Features.Coupon.Commands.ApplyCoupon;

public class ApplyCouponCommandValidator : AbstractValidator<ApplyCouponCommand>
{
    public ApplyCouponCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Coupon code is required.")
            .MaximumLength(50).WithMessage("Coupon code must not exceed 50 characters.");
    }
}
