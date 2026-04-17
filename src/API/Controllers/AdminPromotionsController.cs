using API.Common;
using Application.Common.Models;
using Application.Features.Promotion.Commands.CreatePromotion;
using Application.Features.Promotion.Queries.GetPromotionById;
using Application.Features.Promotion.Queries.GetPromotions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/v1/admin/promotions")]
[Authorize(Policy = "AdminOnly")]
public class AdminPromotionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminPromotionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetPromotionsQuery(page, pageSize));
        return Ok(ApiResponse<PagedResult<PromotionDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] long id)
    {
        var result = await _mediator.Send(new GetPromotionByIdQuery(id));
        return Ok(ApiResponse<PromotionDetailDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePromotionCommand? command)
    {
        if (command is null)
            return BadRequest(ApiResponse<object?>.Fail("Request body is required."));

        var result = await _mediator.Send(command);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<CreatePromotionResult>.Ok(result));
    }
}
