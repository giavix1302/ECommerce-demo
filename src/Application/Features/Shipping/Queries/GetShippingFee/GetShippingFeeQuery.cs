using MediatR;

namespace Application.Features.Shipping.Queries.GetShippingFee;

public record GetShippingFeeQuery(
    string DeliveryAddress,
    double DeliveryLat,
    double DeliveryLng
) : IRequest<IEnumerable<ShippingFeeDto>>;

public record ShippingFeeDto(
    string ServiceId,
    decimal TotalPrice,
    double Distance,
    int Duration
);
