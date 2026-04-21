using Application.Common.DTOs;
using Application.Common.Interfaces;
using Application.Common.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.Promotion.Commands.CreatePromotion;

public class CreatePromotionCommandHandler : IRequestHandler<CreatePromotionCommand, CreatePromotionResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PromotionEngineService _promotionEngine;

    public CreatePromotionCommandHandler(IUnitOfWork unitOfWork, PromotionEngineService promotionEngine)
    {
        _unitOfWork = unitOfWork;
        _promotionEngine = promotionEngine;
    }

    public async Task<CreatePromotionResult> Handle(CreatePromotionCommand request, CancellationToken cancellationToken)
    {
        var promotion = new PromotionRule
        {
            Name = request.Name,
            Type = request.Type,
            Priority = request.Priority,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            AllowCoupon = request.AllowCoupon,
            Conditions = request.Conditions.Select(c => new PromotionCondition
            {
                ConditionType = c.ConditionType,
                TargetId = c.TargetId,
                Value = c.Value
            }).ToList(),
            Actions = request.Actions.Select(a => new PromotionAction
            {
                ActionType = a.ActionType,
                DiscountValue = a.DiscountValue,
                GiftProductId = a.GiftProductId,
                GiftQuantity = a.GiftQuantity
            }).ToList()
        };

        await _unitOfWork.Promotions.AddAsync(promotion);
        await _unitOfWork.SaveChangesAsync();

        _promotionEngine.InvalidateCache();

        return new CreatePromotionResult(
            promotion.Id,
            promotion.Name,
            promotion.Type,
            promotion.Priority,
            promotion.StartDate,
            promotion.EndDate,
            promotion.IsActive,
            promotion.AllowCoupon,
            promotion.Conditions.Select(c => new PromotionConditionDto(c.Id, c.ConditionType, c.TargetId, c.Value)),
            promotion.Actions.Select(a => new PromotionActionDto(a.Id, a.ActionType, a.DiscountValue, a.GiftProductId, a.GiftQuantity))
        );
    }
}