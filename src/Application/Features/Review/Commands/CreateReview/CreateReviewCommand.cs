using MediatR;

namespace Application.Features.Review.Commands.CreateReview;

public record CreateReviewCommand(
    long ProductId,
    long OrderId,
    byte Rating,
    string? Comment
) : IRequest<CreateReviewResult>;

public record CreateReviewResult(long Id);
