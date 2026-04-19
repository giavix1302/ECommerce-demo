using Domain.Enums;
using FluentValidation;

namespace Application.Features.Order.Commands.Checkout;

public class CheckoutCommandValidator : AbstractValidator<CheckoutCommand>
{
    public CheckoutCommandValidator()
    {
        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Invalid payment method.");

        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("Shipping address is required.")
            .MaximumLength(500).WithMessage("Shipping address must not exceed 500 characters.");

        // SelectedGiftVariantId — validated in handler (requires DB lookup)
        // Only basic format check here
        RuleFor(x => x.SelectedGiftVariantId)
            .GreaterThan(0).WithMessage("SelectedGiftVariantId must be a valid id.")
            .When(x => x.SelectedGiftVariantId.HasValue);
    }
}
