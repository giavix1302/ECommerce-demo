using FluentValidation;

namespace Application.Features.Review.Commands.CreateReview;

public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("ProductId is required.");

        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("OrderId is required.");

        RuleFor(x => x.Rating)
            .InclusiveBetween((byte)1, (byte)5).WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Comment must not exceed 1000 characters.")
            .When(x => x.Comment is not null);
    }
}
