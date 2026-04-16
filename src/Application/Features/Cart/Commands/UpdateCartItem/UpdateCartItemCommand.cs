using MediatR;

namespace Application.Features.Cart.Commands.UpdateCartItem;

public record UpdateCartItemCommand(
    long CartItemId,
    int Quantity
) : IRequest;

