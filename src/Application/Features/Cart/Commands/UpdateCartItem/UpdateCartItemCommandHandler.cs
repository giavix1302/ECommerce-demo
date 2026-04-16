using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Cart.Commands.UpdateCartItem;

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCartItemCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var cart = await _unitOfWork.Carts.GetByUserIdWithItemsAsync(userId);

        if (cart == null)
            throw new NotFoundException("Cart not found for the user.");

        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == request.CartItemId);

        if (cartItem == null)
            throw new NotFoundException("Cart item not found.");

        var variant = await _unitOfWork.Variants.GetByIdAsync(cartItem.VariantId);
        if (variant == null || variant.IsDeleted)
            throw new NotFoundException("Product variant not found.");

        if (request.Quantity > variant.StockQuantity)
            throw new BadRequestException("Insufficient stock for the requested quantity.");

        if (request.Quantity == 0)
        {
            cart.CartItems.Remove(cartItem);
        }
        else if (request.Quantity < 0)
        {
            throw new BadRequestException("Quantity cannot be negative.");
        }
        else
        {
            cartItem.Quantity = request.Quantity;
        }
        await _unitOfWork.SaveChangesAsync();
    }
}