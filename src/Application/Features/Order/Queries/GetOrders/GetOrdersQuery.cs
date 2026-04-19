using Application.Common.Models;
using Domain.Enums;
using MediatR;

namespace Application.Features.Order.Queries.GetOrders;

public record GetOrdersQuery(
    int Page = 1,
    int PageSize = 10
) : IRequest<PagedResult<OrderSummaryDto>>;

public record OrderSummaryDto(
    long Id,
    decimal Subtotal,
    decimal TotalAmount,
    OrderStatus Status,
    PaymentStatus PaymentStatus,
    PaymentMethod PaymentMethod,
    DateTime CreatedAt
);
