using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Order.Queries.GetOrders;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetOrdersQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<OrderSummaryDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var (items, totalCount) = await _unitOfWork.Orders.GetPagedByUserIdAsync(userId, request.Page, request.PageSize);

        var dtos = items.Select(o => new OrderSummaryDto(
            o.Id,
            o.Subtotal,
            o.TotalAmount,
            o.Status,
            o.PaymentStatus,
            o.PaymentMethod,
            o.CreatedAt
        ));

        return new PagedResult<OrderSummaryDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
