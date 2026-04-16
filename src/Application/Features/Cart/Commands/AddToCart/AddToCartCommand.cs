using MediatR;

namespace Application.Features.Cart.Commands.AddToCart;

public record AddToCartCommand(
    long ProductVariantId,
    int Quantity
) : IRequest<AddToCartResult>;

public record AddToCartResult(
    long CartItemId,
    long ProductVariantId,
    int Quantity
);