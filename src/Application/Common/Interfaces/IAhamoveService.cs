using Application.Common.DTOs.Ahamove;

namespace Application.Common.Interfaces;

public interface IAhamoveService
{
    Task<IEnumerable<AhamoveServiceEstimate>> EstimateShippingFeeAsync(
        AhamoveEstimateRequest request,
        CancellationToken ct = default);

    Task<AhamoveOrderResponse> CreateOrderAsync(
        AhamoveCreateOrderRequest request,
        CancellationToken ct = default);
}
