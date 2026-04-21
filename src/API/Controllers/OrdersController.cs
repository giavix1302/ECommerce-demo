using API.Common;
using Application.Common.Models;
using Application.Features.Order.Commands.Checkout;
using Application.Features.Order.Queries.GetCheckoutPreview;
using Application.Features.Order.Queries.GetOrderById;
using Application.Features.Order.Queries.GetOrders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public record CheckoutPreviewRequest(
    string DeliveryAddress,
    double DeliveryLat,
    double DeliveryLng
);

[ApiController]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("api/v1/cart/checkout-preview")]
    public async Task<IActionResult> CheckoutPreview([FromBody] CheckoutPreviewRequest request)
    {
        var result = await _mediator.Send(new GetCheckoutPreviewQuery(
            request.DeliveryAddress,
            request.DeliveryLat,
            request.DeliveryLng));
        return Ok(ApiResponse<CheckoutPreviewDto>.Ok(result));
    }

    [HttpPost("api/v1/orders/checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutCommand? command)
    {
        if (command is null)
            return BadRequest(ApiResponse<object?>.Fail("Request body is required."));

        var result = await _mediator.Send(command);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<CheckoutResult>.Ok(result));
    }

    [HttpGet("api/v1/orders")]
    public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetOrdersQuery(page, pageSize));
        return Ok(ApiResponse<PagedResult<OrderSummaryDto>>.Ok(result));
    }

    [HttpGet("api/v1/orders/{id}")]
    public async Task<IActionResult> GetOrderById([FromRoute] long id)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id));
        return Ok(ApiResponse<OrderDetailDto>.Ok(result));
    }
}
