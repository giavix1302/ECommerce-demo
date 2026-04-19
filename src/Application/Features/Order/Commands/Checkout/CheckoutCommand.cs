using Domain.Enums;
using MediatR;

namespace Application.Features.Order.Commands.Checkout;

public record CheckoutCommand(
    PaymentMethod PaymentMethod,
    string ShippingAddress,
    long? SelectedGiftVariantId     // required when promotion action is GIVE_GIFT
) : IRequest<CheckoutResult>;

public record CheckoutResult(
    long OrderId,
    string? PaymentLink             // null for COD, PayOS payment URL otherwise
);
