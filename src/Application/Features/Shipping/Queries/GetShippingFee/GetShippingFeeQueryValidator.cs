using FluentValidation;

namespace Application.Features.Shipping.Queries.GetShippingFee;

public class GetShippingFeeQueryValidator : AbstractValidator<GetShippingFeeQuery>
{
    public GetShippingFeeQueryValidator()
    {
        RuleFor(x => x.DeliveryAddress)
            .NotEmpty().WithMessage("Delivery address is required.");

        RuleFor(x => x.DeliveryLat)
            .NotEqual(0).WithMessage("Delivery latitude is required.");

        RuleFor(x => x.DeliveryLng)
            .NotEqual(0).WithMessage("Delivery longitude is required.");
    }
}
