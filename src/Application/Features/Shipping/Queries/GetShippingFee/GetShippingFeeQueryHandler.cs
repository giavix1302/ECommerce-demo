using Application.Common.DTOs.Ahamove;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace Application.Features.Shipping.Queries.GetShippingFee;

public class GetShippingFeeQueryHandler : IRequestHandler<GetShippingFeeQuery, IEnumerable<ShippingFeeDto>>
{
    private readonly IAhamoveService _ahamove;
    private readonly AhamovePickupOptions _pickup;

    public GetShippingFeeQueryHandler(IAhamoveService ahamove, IOptions<AhamovePickupOptions> pickup)
    {
        _ahamove = ahamove;
        _pickup = pickup.Value;
    }

    public async Task<IEnumerable<ShippingFeeDto>> Handle(GetShippingFeeQuery request, CancellationToken cancellationToken)
    {
        var estimateRequest = new AhamoveEstimateRequest
        {
            Path =
            [
                new AhamoveOrderPath
                {
                    Lat = _pickup.Lat,
                    Lng = _pickup.Lng,
                    Address = _pickup.Address,
                    Name = _pickup.Name,
                    Mobile = _pickup.Mobile
                },
                new AhamoveOrderPath
                {
                    Lat = request.DeliveryLat,
                    Lng = request.DeliveryLng,
                    Address = request.DeliveryAddress,
                    Name = string.Empty,
                    Mobile = string.Empty
                }
            ],
            Services =
            [
                new AhamoveEstimateService { Id = "SGN-BIKE" },
                new AhamoveEstimateService { Id = "SGN-EXPRESS" }
            ]
        };

        var estimates = await _ahamove.EstimateShippingFeeAsync(estimateRequest, cancellationToken);

        return estimates.Select(e => new ShippingFeeDto(
            e.ServiceId,
            e.TotalPrice,
            e.Distance,
            e.Duration
        ));
    }
}

// Options nhỏ chỉ dùng trong handler này — bind từ AhamoveSettings
public class AhamovePickupOptions
{
    public string Address { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lng { get; set; }
}