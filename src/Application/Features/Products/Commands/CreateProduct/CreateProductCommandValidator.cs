using FluentValidation;

namespace Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

        RuleFor(x => x.Variants)
            .NotEmpty().WithMessage("At least one product variant is required.")
            .Must(variants => variants.Any(v => v.IsDefault))
            .WithMessage("At least one variant must be marked as default.")
            .Must(variants => variants.Count(v => v.IsDefault) == 1)
            .WithMessage("Only one variant can be marked as default.");

    }
}
