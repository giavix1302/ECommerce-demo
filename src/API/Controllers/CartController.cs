using API.Common;
using Application.Features.Cart.Commands.AddToCart;
using Application.Features.Cart.Commands.UpdateCartItem;
using Application.Features.Cart.Queries.GetCart;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/v1/carts")]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetCart()
    {
        var result = await _mediator.Send(new GetCartQuery());
        return Ok(ApiResponse<CartDto>.Ok(result));
    }

    [HttpPost("add")]
    [Authorize]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<AddToCartResult>.Ok(result));
    }

    [HttpPut("items/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateCartItem([FromRoute] long id, [FromBody] UpdateCartItemCommand command)
    {
        var cmd = command with { CartItemId = id };
        await _mediator.Send(cmd);
        return Ok(ApiResponse<object?>.Ok(null, "Cart item updated successfully."));
    }
}