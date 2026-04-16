using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Coupon.Commands.CreateCoupon;

public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, CreateCouponResult>
{

    private readonly IUnitOfWork _unitOfWork;

    public CreateCouponCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateCouponResult> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        // Check if a coupon with the same code already exists
        var existingCoupon = await _unitOfWork.Coupons.GetByCodeAsync(request.Code);

        if (existingCoupon != null)
        {
            throw new ConflictException($"A coupon with the code '{request.Code}' already exists.");
        }

        // Create a new Coupon entity
        var coupon = new Domain.Entities.Coupon
        {
            Code = request.Code,
            DiscountType = request.DiscountType,
            Value = request.Value,
            MinOrderValue = request.MinOrderValue,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            UsageLimit = request.UsageLimit
        };

        // Add the new coupon to the repository
        await _unitOfWork.Coupons.AddAsync(coupon);
        await _unitOfWork.SaveChangesAsync();

        return new CreateCouponResult(coupon.Id, coupon.Code);
    }
}