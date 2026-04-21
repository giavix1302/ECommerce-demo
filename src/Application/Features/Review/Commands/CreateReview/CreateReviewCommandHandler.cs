using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Review.Commands.CreateReview;

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, CreateReviewResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateReviewCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CreateReviewResult> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var hasCompletedOrder = await _unitOfWork.Reviews
            .HasCompletedOrderForProductAsync(userId, request.ProductId, request.OrderId);

        if (!hasCompletedOrder)
            throw new BadRequestException(
                "You can only review products from orders that have been successfully delivered.");

        var alreadyReviewed = await _unitOfWork.Reviews
            .HasUserReviewedProductInOrderAsync(userId, request.ProductId, request.OrderId);

        if (alreadyReviewed)
            throw new ConflictException("You have already reviewed this product for the specified order.");

        var review = new Domain.Entities.Review
        {
            UserId = userId,
            ProductId = request.ProductId,
            OrderId = request.OrderId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        await _unitOfWork.Reviews.AddAsync(review);
        await _unitOfWork.SaveChangesAsync();

        return new CreateReviewResult(review.Id);
    }
}
