using FluentValidation;

namespace Application.Features.Variants.Commands.CreateVariant;

public class CreateVariantCommandValidator : AbstractValidator<CreateVariantCommand>
{
    public CreateVariantCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("ProductId is required.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");

        RuleFor(x => x.Sku)
            .MaximumLength(100).WithMessage("SKU must not exceed 100 characters.")
            .When(x => x.Sku is not null);

        RuleForEach(x => x.Attributes).ChildRules(attr =>
        {
            attr.RuleFor(a => a.AttributeId).GreaterThan(0).WithMessage("AttributeId must be valid.");
            attr.RuleFor(a => a.Value).NotEmpty().WithMessage("Attribute value is required.");
        });
    }
}
