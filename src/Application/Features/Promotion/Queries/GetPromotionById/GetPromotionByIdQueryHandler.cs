using Application.Common.DTOs;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Promotion.Queries.GetPromotionById;

public class GetPromotionByIdQueryHandler : IRequestHandler<GetPromotionByIdQuery, PromotionDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPromotionByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PromotionDetailDto> Handle(GetPromotionByIdQuery request, CancellationToken cancellationToken)
    {
        var rule = await _unitOfWork.Promotions.GetPromotionRuleWithDetailsAsync(request.Id);

        if (rule is null)
            throw new NotFoundException("PromotionRule", request.Id);

        return new PromotionDetailDto(
            rule.Id,
            rule.Name,
            rule.Type,
            rule.Priority,
            rule.StartDate,
            rule.EndDate,
            rule.IsActive,
            rule.AllowCoupon,
            rule.Conditions.Select(c => new PromotionConditionDto(c.Id, c.ConditionType, c.TargetId, c.Value)),
            rule.Actions.Select(a => new PromotionActionDto(a.Id, a.ActionType, a.DiscountValue, a.GiftProductId, a.GiftQuantity))
        );
    }
}
