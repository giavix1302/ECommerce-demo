using FluentValidation;

namespace Application.Features.Order.Queries.GetCheckoutPreview;

public class GetCheckoutPreviewQueryValidator : AbstractValidator<GetCheckoutPreviewQuery>
{
    public GetCheckoutPreviewQueryValidator()
    {
        RuleFor(x => x.DeliveryAddress)
            .NotEmpty().WithMessage("Delivery address is required.");

        RuleFor(x => x.DeliveryLat)
            .NotEqual(0).WithMessage("Delivery latitude is required.");

        RuleFor(x => x.DeliveryLng)
            .NotEqual(0).WithMessage("Delivery longitude is required.");
    }
}
