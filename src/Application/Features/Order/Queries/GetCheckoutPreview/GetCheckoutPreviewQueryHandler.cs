using Application.Common.DTOs.Ahamove;
using Application.Common.Exceptions;
using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Services;
using Application.Features.Shipping.Queries.GetShippingFee;
using Domain.Constants;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Options;

namespace Application.Features.Order.Queries.GetCheckoutPreview;

public class GetCheckoutPreviewQueryHandler : IRequestHandler<GetCheckoutPreviewQuery, CheckoutPreviewDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly PromotionEngineService _promotionEngine;
    private readonly IAhamoveService _ahamove;
    private readonly AhamovePickupOptions _pickup;

    public GetCheckoutPreviewQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        PromotionEngineService promotionEngine,
        IAhamoveService ahamove,
        IOptions<AhamovePickupOptions> pickup)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _promotionEngine = promotionEngine;
        _ahamove = ahamove;
        _pickup = pickup.Value;
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

        // 8. Estimate shipping fee từ Ahamove (trả list để client chọn service)
        var bulkyTier = AhamoveBulkyTierHelper.GetTierFromItems(
            cart.CartItems.Select(ci => (ci.Variant.WeightKg, ci.Variant.LengthCm, ci.Variant.WidthCm, ci.Variant.HeightCm, ci.Quantity))
        );
        if (bulkyTier.ExceedsLimit)
            throw new BadRequestException("Order is too heavy or too large for motorbike delivery. Please contact us for alternative shipping.");
        var shippingEstimates = await EstimateShippingAsync(request, bulkyTier.Tier, cancellationToken);

        var totalAmount = baseAmount - rankDiscountAmount - couponDiscount;

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
            shippingEstimates,
            totalAmount
        );
    }

    private async Task<IEnumerable<CheckoutShippingEstimateDto>> EstimateShippingAsync(
        GetCheckoutPreviewQuery request,
        string? bulkyTier,
        CancellationToken ct)
    {
        try
        {
            List<AhamoveEstimateService> services =
            [
                new AhamoveEstimateService
                {
                    Id = "SGN-BIKE",
                    Requests = bulkyTier is not null
                        ? [new AhamoveBulkyRequest { Id = "SGN-BIKE-BULKY", TierCode = bulkyTier }]
                        : []
                },
                new AhamoveEstimateService
                {
                    Id = "SGN-EXPRESS",
                    Requests = bulkyTier is not null
                        ? [new AhamoveBulkyRequest { Id = "SGN-EXPRESS-BULKY", TierCode = bulkyTier }]
                        : []
                }
            ];

            var estimateRequest = new AhamoveEstimateRequest
            {
                Path =
                [
                    new AhamoveOrderPath
                    {
                        Lat = _pickup.Lat,
                        Lng = _pickup.Lng,
                        Address = _pickup.Address,
                        Name = _pickup.Name,
                        Mobile = _pickup.Mobile
                    },
                    new AhamoveOrderPath
                    {
                        Lat = request.DeliveryLat,
                        Lng = request.DeliveryLng,
                        Address = request.DeliveryAddress,
                        Name = string.Empty,
                        Mobile = string.Empty
                    }
                ],
                Services = services
            };

            var estimates = await _ahamove.EstimateShippingFeeAsync(estimateRequest, ct);
            return estimates
                .Where(e => e.Data is not null)
                .Select(e => new CheckoutShippingEstimateDto(
                    e.ServiceId,
                    e.TotalPrice,
                    e.Distance,
                    e.Duration
                ));
        }
        catch
        {
            return [];
        }
    }
}
