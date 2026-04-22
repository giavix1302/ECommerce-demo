using API.Common;
using API.RateLimit;
using Application.Features.Categories.Commands.CreateCategory;
using Application.Features.Categories.Commands.DeleteCategory;
using Application.Features.Categories.Commands.UpdateCategory;
using Application.Features.Categories.Queries.GetCategories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/v1/admin/categories")]
[Authorize(Policy = "AdminOnly")]
public class AdminCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminCategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [EnableRateLimiting(RateLimitPolicy.Read)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetCategoriesQuery());
        return Ok(ApiResponse<IEnumerable<CategoryDto>>.Ok(result));
    }

    [HttpPost]
    [EnableRateLimiting(RateLimitPolicy.Write)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryCommand? command)
    {
        if (command is null)
            return BadRequest(ApiResponse<object?>.Fail("Request body is required."));

        var result = await _mediator.Send(command);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<CreateCategoryResult>.Ok(result));
    }

    [HttpPut("{id}")]
    [EnableRateLimiting(RateLimitPolicy.Write)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateCategoryCommand? command)
    {
        if (command is null)
            return BadRequest(ApiResponse<object?>.Fail("Request body is required."));

        if (id != command.Id)
            return BadRequest(ApiResponse<object?>.Fail("ID in route does not match ID in body."));

        await _mediator.Send(command);
        return Ok(ApiResponse<object?>.Ok(null, "Category updated successfully."));
    }

    [HttpDelete("{id}")]
    [EnableRateLimiting(RateLimitPolicy.Write)]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        await _mediator.Send(new DeleteCategoryCommand(id));
        return Ok(ApiResponse<object?>.Ok(null, "Category deleted successfully."));
    }
}
