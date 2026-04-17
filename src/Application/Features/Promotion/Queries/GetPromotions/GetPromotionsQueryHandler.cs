using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Promotion.Queries.GetPromotions;

public class GetPromotionsQueryHandler : IRequestHandler<GetPromotionsQuery, PagedResult<PromotionDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPromotionsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<PromotionDto>> Handle(GetPromotionsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.Promotions.GetPagedAsync(request.Page, request.PageSize);

        var dtos = items.Select(r => new PromotionDto(
            r.Id,
            r.Name,
            r.Type,
            r.Priority,
            r.StartDate,
            r.EndDate,
            r.IsActive,
            r.AllowCoupon
        ));

        return new PagedResult<PromotionDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
