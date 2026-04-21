using Domain.Enums;
using FluentValidation;

namespace Application.Features.Order.Commands.Checkout;

public class CheckoutCommandValidator : AbstractValidator<CheckoutCommand>
{
    public CheckoutCommandValidator()
    {
        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Invalid payment method.");

        RuleFor(x => x.DeliveryAddress)
            .NotEmpty().WithMessage("Delivery address is required.")
            .MaximumLength(500).WithMessage("Delivery address must not exceed 500 characters.");

        RuleFor(x => x.DeliveryLat)
            .NotEqual(0).WithMessage("Delivery latitude is required.");

        RuleFor(x => x.DeliveryLng)
            .NotEqual(0).WithMessage("Delivery longitude is required.");

        // SelectedGiftVariantId — validated in handler (requires DB lookup)
        // Only basic format check here
        RuleFor(x => x.SelectedGiftVariantId)
            .GreaterThan(0).WithMessage("SelectedGiftVariantId must be a valid id.")
            .When(x => x.SelectedGiftVariantId.HasValue);
    }
}
