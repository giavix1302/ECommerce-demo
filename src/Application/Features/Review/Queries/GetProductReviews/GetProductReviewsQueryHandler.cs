using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Review.Queries.GetProductReviews;

public class GetProductReviewsQueryHandler : IRequestHandler<GetProductReviewsQuery, GetProductReviewsResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductReviewsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetProductReviewsResult> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.Reviews
            .GetPagedByProductIdAsync(request.ProductId, request.Page, request.PageSize);

        var averageRating = await _unitOfWork.Reviews.GetAverageRatingAsync(request.ProductId);

        var dtos = items.Select(r => new ReviewDto(
            r.Id,
            r.UserId,
            r.User.FullName,
            r.Rating,
            r.Comment,
            r.CreatedAt
        ));

        var pagedResult = new PagedResult<ReviewDto>(dtos, totalCount, request.Page, request.PageSize);

        return new GetProductReviewsResult(pagedResult, Math.Round(averageRating, 1));
    }
}
