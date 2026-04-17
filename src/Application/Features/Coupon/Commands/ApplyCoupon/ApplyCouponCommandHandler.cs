using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Coupon.Commands.ApplyCoupon;

public class ApplyCouponCommandHandler : IRequestHandler<ApplyCouponCommand, ApplyCouponResult>
{

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ApplyCouponCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApplyCouponResult> Handle(ApplyCouponCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var currentCoupon = await _unitOfWork.Coupons.GetByCodeAsync(request.Code);
        if (currentCoupon == null)
            throw new NotFoundException("Coupon code not found.");

        if (!currentCoupon.IsActive || currentCoupon.StartDate > DateTime.UtcNow || currentCoupon.EndDate < DateTime.UtcNow)
            throw new BadRequestException("Coupon is not active.");

        if (currentCoupon.UsageLimit.HasValue && currentCoupon.UsedCount >= currentCoupon.UsageLimit.Value)
            throw new BadRequestException("Coupon usage limit has been reached.");

        var cart = await _unitOfWork.Carts.GetByUserIdWithItemsAsync(userId);

        if (cart is null || !cart.CartItems.Any())
            throw new BadRequestException("Your cart is empty.");

        var cartTotal = cart.CartItems.Sum(ci => ci.Quantity * ci.UnitPrice);
        if (cartTotal < currentCoupon.MinOrderValue)
            throw new BadRequestException($"Minimum order value for this coupon is {currentCoupon.MinOrderValue}.");

        if (await _unitOfWork.Coupons.HasUserUsedCouponAsync(currentCoupon.Id, userId))
            throw new BadRequestException("You have already used this coupon.");

        decimal discountAmount = currentCoupon.DiscountType switch
        {
            Domain.Enums.DiscountType.PERCENTAGE => cartTotal * (currentCoupon.Value / 100),
            Domain.Enums.DiscountType.FIXED_AMOUNT => currentCoupon.Value,
            _ => 0
        };

        cart.CouponId = currentCoupon.Id;
        cart.DiscountAmount = discountAmount;
        _unitOfWork.Carts.Update(cart);
        await _unitOfWork.SaveChangesAsync();

        return new ApplyCouponResult(discountAmount);

    }
}
