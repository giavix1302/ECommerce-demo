using API.Common;
using Application.Common.DTOs.Ahamove;
using Application.Features.Shipping.Commands.ProcessAhamoveWebhook;
using Application.Features.Shipping.Queries.GetShippingFee;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/v1/shipping")]
public class ShippingController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShippingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("fee")]
    [Authorize]
    public async Task<IActionResult> GetShippingFee([FromBody] GetShippingFeeRequest request)
    {
        var result = await _mediator.Send(new GetShippingFeeQuery(
            request.DeliveryAddress,
            request.DeliveryLat,
            request.DeliveryLng
        ));
        return Ok(ApiResponse<IEnumerable<ShippingFeeDto>>.Ok(result));
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> AhamoveWebhook([FromBody] AhamoveWebhookPayload payload)
    {
        try
        {
            await _mediator.Send(new ProcessAhamoveWebhookCommand(payload));
        }
        catch
        {
            // always 200 — Ahamove retries on non-2xx
        }
        return Ok();
    }
}

public record GetShippingFeeRequest(
    string DeliveryAddress,
    double DeliveryLat,
    double DeliveryLng
);
