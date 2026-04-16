using Application.Common.DTOs;
using MediatR;

namespace Application.Features.Cart.Queries.GetCart;

public record GetCartQuery : IRequest<CartDto>;

public record CartDto(
    long Id,
    long UserId,
    decimal TotalPrice,
    IEnumerable<CartItemDto> Items,
    CartCouponDto? Coupon
);

public record CartItemDto(
    long Id,
    long VariantId,
    string ProductName,
    string? Sku,
    decimal UnitPrice,
    int Quantity,
    decimal SubTotal,
    IEnumerable<VariantAttributeDto> Attributes
);

public record CartCouponDto(string Code, decimal? DiscountAmount);