using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Coupon.Commands.RemoveCoupon;

public class RemoveCouponCommandHandler : IRequestHandler<RemoveCouponCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RemoveCouponCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(RemoveCouponCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var cart = await _unitOfWork.Carts.GetByUserIdWithItemsAsync(userId);

        if (cart is null)
            throw new NotFoundException("Cart not found.");

        if (cart.CouponId is null)
            throw new BadRequestException("No coupon applied to your cart.");

        cart.CouponId = null;
        cart.DiscountAmount = 0;
        _unitOfWork.Carts.Update(cart);
        await _unitOfWork.SaveChangesAsync();
    }
}
