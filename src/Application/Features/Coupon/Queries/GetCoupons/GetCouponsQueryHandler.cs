using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Coupon.Queries.GetCoupons;

public class GetCouponsQueryHandler : IRequestHandler<GetCouponsQuery, PagedResult<CouponDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCouponsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<CouponDto>> Handle(GetCouponsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.Coupons.GetPagedAsync(
            request.Page,
            request.PageSize);

        var dtos = items.Select(c => new CouponDto(
            c.Id,
            c.Code,
            c.DiscountType.ToString(),
            c.Value,
            c.MinOrderValue,
            c.StartDate,
            c.EndDate,
            c.UsageLimit,
            c.UsedCount,
            c.IsActive
        ));

        return new PagedResult<CouponDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}

