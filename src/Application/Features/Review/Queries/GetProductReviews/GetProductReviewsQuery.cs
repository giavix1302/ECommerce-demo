using Application.Common.Models;
using MediatR;

namespace Application.Features.Review.Queries.GetProductReviews;

public record GetProductReviewsQuery(
    long ProductId,
    int Page = 1,
    int PageSize = 10
) : IRequest<GetProductReviewsResult>;

public record GetProductReviewsResult(
    PagedResult<ReviewDto> Reviews,
    double AverageRating
);

public record ReviewDto(
    long Id,
    long UserId,
    string UserName,
    byte Rating,
    string? Comment,
    DateTime CreatedAt
);
