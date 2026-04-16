
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Cart.Commands.AddToCart;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, AddToCartResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AddToCartCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<AddToCartResult> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // check variant stock
        var variant = await _unitOfWork.Variants.GetByIdAsync(request.ProductVariantId);
        if (variant == null || variant.IsDeleted)
            throw new NotFoundException("ProductVariant", request.ProductVariantId);

        if (variant.StockQuantity < request.Quantity)
            throw new BadRequestException("Insufficient stock.");

        // get or create cart for user
        var cart = await _unitOfWork.Carts.GetByUserIdWithItemsAsync(userId);
        if (cart == null)
        {
            cart = new Domain.Entities.Cart
            {
                UserId = userId,
            };
            await _unitOfWork.Carts.AddAsync(cart);
        }
        // check cart item exists, if exists update quantity, else add new cart item
        // Tìm xem item với variantId này đã có trong giỏ chưa
        var existingItem = cart.CartItems.FirstOrDefault(i => i.VariantId == request.ProductVariantId);

        CartItem? newCartItem = null;
        if (existingItem is not null)
        {
            var newQuantity = existingItem.Quantity + request.Quantity;
            if (newQuantity > variant.StockQuantity)
                throw new BadRequestException("Insufficient stock for the requested quantity.");
            existingItem.Quantity = newQuantity;
        }
        else
        {
            newCartItem = new Domain.Entities.CartItem
            {
                VariantId = request.ProductVariantId,
                Quantity = request.Quantity,
                UnitPrice = variant.Price // snapshot giá tại thời điểm thêm vào giỏ
            };
            cart.CartItems.Add(newCartItem);
        }

        // save changes
        await _unitOfWork.SaveChangesAsync();
        return new AddToCartResult(
            existingItem?.Id ?? newCartItem!.Id,
            request.ProductVariantId,
            existingItem?.Quantity ?? request.Quantity
        );
    }
}