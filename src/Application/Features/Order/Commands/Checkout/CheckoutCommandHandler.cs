using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Services;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Order.Commands.Checkout;

public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, CheckoutResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly PromotionEngineService _promotionEngine;

    public CheckoutCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        PromotionEngineService promotionEngine)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _promotionEngine = promotionEngine;
    }

    public async Task<CheckoutResult> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // 1. Load cart với đầy đủ data
        var cart = await _unitOfWork.Carts.GetByUserIdWithItemsAsync(userId);
        if (cart is null || !cart.CartItems.Any())
            throw new BadRequestException("Your cart is empty.");

        // 2. Validate stock lần cuối
        foreach (var item in cart.CartItems)
        {
            if (item.Variant.StockQuantity < item.Quantity)
                throw new BadRequestException(
                    $"Insufficient stock for variant {item.VariantId}. Available: {item.Variant.StockQuantity}.");
        }

        // 3. Tính Subtotal
        var subtotal = cart.CartItems.Sum(ci => ci.UnitPrice * ci.Quantity);

        // 4. Chạy PromotionEngine
        var (bestRule, promotionDiscount) = await _promotionEngine.EvaluateAsync(cart);

        // 5. Validate GIVE_GIFT — SelectedGiftVariantId bắt buộc
        ProductVariant? giftVariant = null;
        PromotionAction? giftAction = null;
        if (bestRule is not null)
        {
            giftAction = bestRule.Actions.First();
            if (giftAction.ActionType == PromotionActionType.GIVE_GIFT)
            {
                if (!request.SelectedGiftVariantId.HasValue)
                    throw new BadRequestException("SelectedGiftVariantId is required for GIVE_GIFT promotion.");

                giftVariant = await _unitOfWork.Variants.GetByIdAsync(request.SelectedGiftVariantId.Value);

                if (giftVariant is null || giftVariant.ProductId != giftAction.GiftProductId)
                    throw new BadRequestException("Invalid gift variant selected.");

                if (giftVariant.StockQuantity < (giftAction.GiftQuantity ?? 1))
                    throw new BadRequestException("Gift variant is out of stock.");
            }
        }

        // 6. Load User → MembershipRank
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException("User", userId);

        var rankDiscountRate = user.MembershipRank switch
        {
            MembershipRank.GOLD => MembershipPolicy.GoldDiscount,
            MembershipRank.DIAMOND => MembershipPolicy.DiamondDiscount,
            _ => MembershipPolicy.SilverDiscount
        };

        // 7. Tính discount theo thứ tự: Promotion → Rank → Coupon
        var baseAmount = subtotal - promotionDiscount;
        var rankDiscountAmount = baseAmount * rankDiscountRate;

        decimal couponDiscount = 0;
        if (cart.CouponId.HasValue && cart.Coupon is not null)
        {
            if (bestRule is null)
                couponDiscount = cart.DiscountAmount;
            else if (bestRule.AllowCoupon)
                couponDiscount = cart.Coupon.DiscountType switch
                {
                    DiscountType.PERCENTAGE => baseAmount * cart.Coupon.Value / 100,
                    DiscountType.FIXED_AMOUNT => cart.Coupon.Value,
                    _ => 0
                };
        }

        var totalAmount = baseAmount - rankDiscountAmount - couponDiscount;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // 8. Trừ stock tất cả variant trong cart
            foreach (var item in cart.CartItems)
            {
                item.Variant.StockQuantity -= item.Quantity;
                _unitOfWork.Variants.Update(item.Variant);
            }

            // Trừ stock gift variant (nếu có)
            if (giftVariant is not null && giftAction is not null)
            {
                giftVariant.StockQuantity -= giftAction.GiftQuantity ?? 1;
                _unitOfWork.Variants.Update(giftVariant);
            }

            // 9. Tạo Order
            var paymentExpiredAt = request.PaymentMethod == PaymentMethod.PAYOS
                ? DateTime.UtcNow.AddMinutes(15)
                : (DateTime?)null;

            var order = new Domain.Entities.Order
            {
                UserId = userId,
                CouponId = cart.CouponId,
                PromotionRuleId = bestRule?.Id,
                Subtotal = subtotal,
                PromotionDiscountAmount = promotionDiscount,
                RankDiscountAmount = rankDiscountAmount,
                DiscountAmount = couponDiscount,
                ShippingFee = 0,            // TODO: Task 8 — Ahamove
                TotalAmount = totalAmount,
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = PaymentStatus.UNPAID,
                Status = OrderStatus.PENDING,
                ShippingAddress = request.ShippingAddress,
                PaymentExpiredAt = paymentExpiredAt,
            };
            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();   // flush để có order.Id

            // 10. Tạo OrderItems
            var orderItems = cart.CartItems.Select(ci => new OrderItem
            {
                OrderId = order.Id,
                VariantId = ci.VariantId,
                Quantity = ci.Quantity,
                UnitPrice = ci.UnitPrice,
                TotalPrice = ci.UnitPrice * ci.Quantity,
                PromotionRuleId = null
            }).ToList();

            // Gift item — UnitPrice = 0, PromotionRuleId set
            if (giftVariant is not null && giftAction is not null)
            {
                orderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    VariantId = giftVariant.Id,
                    Quantity = giftAction.GiftQuantity ?? 1,
                    UnitPrice = 0,
                    TotalPrice = 0,
                    PromotionRuleId = bestRule!.Id
                });
            }

            foreach (var item in orderItems)
                await _unitOfWork.Orders.AddOrderItemAsync(item);

            // 11. Cập nhật Coupon.UsedCount + tạo CouponUsage
            if (cart.CouponId.HasValue && cart.Coupon is not null && couponDiscount > 0)
            {
                cart.Coupon.UsedCount += 1;
                _unitOfWork.Coupons.Update(cart.Coupon);

                await _unitOfWork.Coupons.AddUsageAsync(new CouponUsage
                {
                    CouponId = cart.Coupon.Id,
                    UserId = userId,
                    OrderId = order.Id
                });
            }

            // 12. Xóa CartItems, reset Cart
            _unitOfWork.Carts.ClearItems(cart);
            cart.CouponId = null;
            cart.DiscountAmount = 0;
            cart.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Carts.Update(cart);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            // COD: TODO Task 8 — gọi Ahamove API tạo Shipment
            if (request.PaymentMethod == PaymentMethod.COD)
            {
                return new CheckoutResult(order.Id, PaymentLink: null);
            }

            // PayOS: tạo PaymentTransaction + payment link
            // TODO: Task 7 — gọi PayOS API lấy payment link thật
            await _unitOfWork.Orders.AddPaymentTransactionAsync(new PaymentTransaction
            {
                OrderId = order.Id,
                PayOSOrderCode = order.Id,      // tạm dùng OrderId, Task 7 sẽ dùng mã PayOS thật
                Amount = totalAmount,
                Status = PaymentTransactionStatus.PENDING,
            });
            await _unitOfWork.SaveChangesAsync();

            // Task 7 sẽ trả về URL thật từ PayOS SDK
            return new CheckoutResult(order.Id, PaymentLink: null);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
