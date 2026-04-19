using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Order.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetOrderByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<OrderDetailDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(request.OrderId);

        if (order is null)
            throw new NotFoundException("Order", request.OrderId);

        // Chỉ cho phép user xem order của chính mình
        if (order.UserId != _currentUserService.UserId)
            throw new NotFoundException("Order", request.OrderId);

        var items = order.OrderItems.Select(oi => new OrderItemDto(
            oi.VariantId,
            oi.Variant.Product.Name,
            oi.Variant.Sku,
            oi.Variant.AttributeValues.Select(av => new OrderItemAttributeDto(av.Attribute.Name, av.Value)),
            oi.UnitPrice,
            oi.Quantity,
            oi.TotalPrice,
            IsGift: oi.PromotionRuleId.HasValue && oi.UnitPrice == 0
        ));

        return new OrderDetailDto(
            order.Id,
            order.Subtotal,
            order.PromotionDiscountAmount,
            order.RankDiscountAmount,
            order.DiscountAmount,
            order.ShippingFee,
            order.TotalAmount,
            order.Status,
            order.PaymentStatus,
            order.PaymentMethod,
            order.ShippingAddress,
            order.CreatedAt,
            items
        );
    }
}
