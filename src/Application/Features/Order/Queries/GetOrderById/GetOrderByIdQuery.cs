using Domain.Enums;
using MediatR;

namespace Application.Features.Order.Queries.GetOrderById;

public record GetOrderByIdQuery(long OrderId) : IRequest<OrderDetailDto>;

public record OrderDetailDto(
    long Id,
    decimal Subtotal,
    decimal PromotionDiscountAmount,
    decimal RankDiscountAmount,
    decimal DiscountAmount,
    decimal ShippingFee,
    decimal TotalAmount,
    OrderStatus Status,
    PaymentStatus PaymentStatus,
    PaymentMethod PaymentMethod,
    string? ShippingAddress,
    DateTime CreatedAt,
    IEnumerable<OrderItemDto> Items
);

public record OrderItemDto(
    long VariantId,
    string ProductName,
    string? Sku,
    IEnumerable<OrderItemAttributeDto> Attributes,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice,
    bool IsGift
);

public record OrderItemAttributeDto(
    string AttributeName,
    string Value
);
