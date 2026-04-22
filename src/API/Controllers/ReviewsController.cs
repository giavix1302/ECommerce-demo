using API.Common;
using API.RateLimit;
using Application.Features.Review.Commands.CreateReview;
using Application.Features.Review.Queries.GetProductReviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/v1")]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("reviews")]
    [Authorize]
    [EnableRateLimiting(RateLimitPolicy.Write)]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewCommand? command)
    {
        if (command is null)
            return BadRequest(ApiResponse<object?>.Fail("Request body is required."));

        var result = await _mediator.Send(command);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<CreateReviewResult>.Ok(result));
    }

    [HttpGet("products/{id:long}/reviews")]
    [EnableRateLimiting(RateLimitPolicy.Read)]
    public async Task<IActionResult> GetProductReviews(
        [FromRoute] long id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetProductReviewsQuery(id, page, pageSize));
        return Ok(ApiResponse<GetProductReviewsResult>.Ok(result));
    }
}
