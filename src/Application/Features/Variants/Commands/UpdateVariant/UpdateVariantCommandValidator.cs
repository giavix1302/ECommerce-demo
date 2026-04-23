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
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(100).WithMessage("SKU must not exceed 100 characters.");

        RuleForEach(x => x.Attributes)
            .ChildRules(attr =>
            {
                attr.RuleFor(a => a.AttributeName)
                    .NotEmpty().WithMessage("Attribute name is required.")
                    .MaximumLength(100).WithMessage("Attribute name must not exceed 100 characters.");
                attr.RuleFor(a => a.Value)
                    .NotEmpty().WithMessage("Attribute value is required.");
            })
            .When(x => x.Attributes is not null);
    }
}
