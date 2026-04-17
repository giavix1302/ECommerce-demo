using Application.Common.DTOs;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Cart.Queries.GetCart;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly ICurrentUserService _currentUserService;

  public GetCartQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
  {
    _unitOfWork = unitOfWork;
    _currentUserService = currentUserService;
  }

  public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
  {
    var userId = _currentUserService.UserId;

    var cart = await _unitOfWork.Carts.GetByUserIdWithItemsAsync(userId);

    if (cart is null)
      return new CartDto(0, userId, 0, new List<CartItemDto>(), null);

    return new CartDto(
      cart.Id,
      userId,
      cart.CartItems.Sum(i => i.UnitPrice * i.Quantity),
      cart.CartItems.Select(i => new CartItemDto(
        i.Id,
        i.VariantId,
        i.Variant.Product.Name,
        i.Variant.Sku,
        i.UnitPrice,
        i.Quantity,
        i.UnitPrice * i.Quantity,
        i.Variant.AttributeValues.Select(av => new VariantAttributeDto(
          av.AttributeId,
          av.Attribute.Name,
          av.Value
        ))
       )
      ),
      cart.Coupon != null ? new CartCouponDto(cart.Coupon.Code, cart.DiscountAmount) : null
    );
  }
}
