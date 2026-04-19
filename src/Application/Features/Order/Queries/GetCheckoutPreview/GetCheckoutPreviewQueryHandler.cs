using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Services;
using Domain.Constants;
using Domain.Enums;
using MediatR;

namespace Application.Features.Order.Queries.GetCheckoutPreview;

public class GetCheckoutPreviewQueryHandler : IRequestHandler<GetCheckoutPreviewQuery, CheckoutPreviewDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly PromotionEngineService _promotionEngine;

    public GetCheckoutPreviewQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        PromotionEngineService promotionEngine)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _promotionEngine = promotionEngine;
    }

    public async Task<CheckoutPreviewDto> Handle(GetCheckoutPreviewQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // 1. Load cart (includes items → variant → product → category, attributes, coupon)
        var cart = await _unitOfWork.Carts.GetByUserIdWithItemsAsync(userId);
        if (cart is null || !cart.CartItems.Any())
            throw new BadRequestException("Your cart is empty.");

        // 2. Tính Subtotal
        var subtotal = cart.CartItems.Sum(ci => ci.UnitPrice * ci.Quantity);

        // 3. Chạy PromotionEngine
        var (bestRule, promotionDiscount) = await _promotionEngine.EvaluateAsync(cart);

        // 4. Load User để biết MembershipRank
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException("User", userId);

        var rankDiscountRate = user.MembershipRank switch
        {
            MembershipRank.GOLD => MembershipPolicy.GoldDiscount,
            MembershipRank.DIAMOND => MembershipPolicy.DiamondDiscount,
            _ => MembershipPolicy.SilverDiscount
        };

        // 5. Base = Subtotal - PromotionDiscount
        var baseAmount = subtotal - promotionDiscount;

        // 6. Tính RankDiscount
        var rankDiscountAmount = baseAmount * rankDiscountRate;

        // 7. Tính CouponDiscount theo 3 case
        decimal couponDiscount = 0;
        if (cart.CouponId.HasValue && cart.Coupon is not null)
        {
            if (bestRule is null)
            {
                // Không có promotion → dùng cart.DiscountAmount trực tiếp
                couponDiscount = cart.DiscountAmount;
            }
            else if (!bestRule.AllowCoupon)
            {
                // Có promotion, AllowCoupon = false → bỏ coupon
                couponDiscount = 0;
            }
            else
            {
                // Có promotion, AllowCoupon = true → tính lại trên Base
                couponDiscount = cart.Coupon.DiscountType switch
                {
                    DiscountType.PERCENTAGE => baseAmount * cart.Coupon.Value / 100,
                    DiscountType.FIXED_AMOUNT => cart.Coupon.Value,
                    _ => 0
                };
            }
        }

        // 8. Tính TotalAmount
        var totalAmount = baseAmount - rankDiscountAmount - couponDiscount;
        // ShippingFee = null — implement sau khi có Ahamove (Task 8)

        // 9. Build DTO
        var items = cart.CartItems.Select(ci => new CheckoutItemDto(
            ci.Id,
            ci.VariantId,
            ci.Variant.Product.Name,
            ci.Variant.Sku,
            ci.Variant.AttributeValues.Select(av => new CheckoutItemAttributeDto(av.Attribute.Name, av.Value)),
            ci.UnitPrice,
            ci.Quantity,
            ci.UnitPrice * ci.Quantity
        ));

        CheckoutPromotionDto? promotionDto = null;
        if (bestRule is not null)
        {
            var action = bestRule.Actions.First();
            CheckoutGiftDto? giftDto = null;

            if (action.ActionType == PromotionActionType.GIVE_GIFT && action.GiftProductId.HasValue)
            {
                // Load GiftProduct với tất cả variants còn stock
                var giftProduct = await _unitOfWork.Products.GetByIdWithVariantsAsync(action.GiftProductId.Value);
                if (giftProduct is not null)
                {
                    var availableVariants = giftProduct.Variants
                        .Where(v => !v.IsDeleted && v.StockQuantity >= (action.GiftQuantity ?? 1))
                        .Select(v => new CheckoutGiftVariantDto(
                            v.Id,
                            v.Sku,
                            v.AttributeValues.Select(av => new CheckoutItemAttributeDto(av.Attribute.Name, av.Value)),
                            v.Price
                        ));

                    giftDto = new CheckoutGiftDto(
                        giftProduct.Id,
                        giftProduct.Name,
                        action.GiftQuantity ?? 1,
                        availableVariants
                    );
                }
            }

            promotionDto = new CheckoutPromotionDto(
                bestRule.Id,
                bestRule.Name,
                bestRule.Type,
                promotionDiscount,
                bestRule.AllowCoupon,
                giftDto
            );
        }

        // Always show coupon if user has applied one — even when blocked (DiscountAmount = 0)
        // Client uses DiscountAmount = 0 to display "coupon not applicable due to current promotion"
        CheckoutCouponDto? couponDto = null;
        if (cart.CouponId.HasValue && cart.Coupon is not null)
        {
            couponDto = new CheckoutCouponDto(cart.Coupon.Code, couponDiscount);
        }

        return new CheckoutPreviewDto(
            items,
            subtotal,
            promotionDto,
            couponDto,
            user.MembershipRank,
            rankDiscountAmount,
            ShippingFee: null,
            totalAmount
        );
    }
}
