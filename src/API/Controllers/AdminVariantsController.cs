using API.Common;
using Application.Features.Variants.Commands.CreateVariant;
using Application.Features.Variants.Commands.DeleteVariant;
using Application.Features.Variants.Commands.UpdateVariant;
using Application.Features.Variants.Queries.GetVariants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/v1/admin/variants")]
[Authorize(Policy = "AdminOnly")]
public class AdminVariantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminVariantsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetByProduct([FromQuery] long productId)
    {
        var result = await _mediator.Send(new GetVariantsQuery(productId));
        return Ok(ApiResponse<IEnumerable<VariantDto>>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVariantCommand? command)
    {
        if (command is null)
            return BadRequest(ApiResponse<object?>.Fail("Request body is required."));

        var result = await _mediator.Send(command);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<CreateVariantResult>.Ok(result));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateVariantCommand? command)
    {
        if (command is null)
            return BadRequest(ApiResponse<object?>.Fail("Request body is required."));

        if (id != command.Id)
            return BadRequest(ApiResponse<object?>.Fail("ID in route does not match ID in body."));

        await _mediator.Send(command);
        return Ok(ApiResponse<object?>.Ok(null, "Variant updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        await _mediator.Send(new DeleteVariantCommand(id));
        return Ok(ApiResponse<object?>.Ok(null, "Variant deleted successfully."));
    }
}
