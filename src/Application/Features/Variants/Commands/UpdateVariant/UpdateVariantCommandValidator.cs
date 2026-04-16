using FluentValidation;

namespace Application.Features.Variants.Commands.UpdateVariant;

public class UpdateVariantCommandValidator : AbstractValidator<UpdateVariantCommand>
{
    public UpdateVariantCommandValidator()
    {
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");

        RuleFor(x => x.Sku)
            .MaximumLength(100).WithMessage("SKU must not exceed 100 characters.")
            .When(x => x.Sku is not null);
    }
}
