using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Coupon.Commands.UpdateCoupon;

public class UpdateCouponCommandHandler : IRequestHandler<UpdateCouponCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCouponCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _unitOfWork.Coupons.GetByIdAsync(request.Id);
        if (coupon is null)
            throw new NotFoundException("Coupon", request.Id);

        coupon.Value = request.Value;
        coupon.MinOrderValue = request.MinOrderValue;
        coupon.StartDate = request.StartDate;
        coupon.EndDate = request.EndDate;
        coupon.UsageLimit = request.UsageLimit;
        coupon.IsActive = request.IsActive;

        _unitOfWork.Coupons.Update(coupon);
        await _unitOfWork.SaveChangesAsync();
    }
}
