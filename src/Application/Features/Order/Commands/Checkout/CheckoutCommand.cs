using Domain.Enums;
using MediatR;

namespace Application.Features.Order.Commands.Checkout;

public record CheckoutCommand(
    PaymentMethod PaymentMethod,
    string DeliveryAddress,
    double DeliveryLat,
    double DeliveryLng,
    string ServiceId,               // Ahamove service: SGN-BIKE, SGN-EXPRESS, ...
    long? SelectedGiftVariantId     // required when promotion action is GIVE_GIFT
) : IRequest<CheckoutResult>;

public record CheckoutResult(
    long OrderId,
    string? PaymentLink             // null for COD, PayOS payment URL otherwise
);
