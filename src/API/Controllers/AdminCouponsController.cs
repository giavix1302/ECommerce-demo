using API.Common;
using Application.Common.Models;
using Application.Features.Coupon.Commands.CreateCoupon;
using Application.Features.Coupon.Commands.UpdateCoupon;
using Application.Features.Coupon.Queries.GetCoupons;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/v1/admin/coupons")]
[Authorize(Policy = "AdminOnly")]
public class AdminCouponsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminCouponsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetCouponsQuery(page, pageSize));
        return Ok(ApiResponse<PagedResult<CouponDto>>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCouponCommand? command)
    {
        if (command is null)
            return BadRequest(ApiResponse<object?>.Fail("Request body is required."));

        var result = await _mediator.Send(command);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<CreateCouponResult>.Ok(result));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateCouponCommand? command)
    {
        if (command is null)
            return BadRequest(ApiResponse<object?>.Fail("Request body is required."));

        await _mediator.Send(command with { Id = id });
        return Ok(ApiResponse<object?>.Ok(null, "Coupon updated successfully."));
    }
}
