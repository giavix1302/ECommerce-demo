using Application.Common.DTOs.Ahamove;
using Application.Common.Exceptions;
using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Services;
using Application.Features.Shipping.Queries.GetShippingFee;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Options;

namespace Application.Features.Order.Commands.Checkout;

public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, CheckoutResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly PromotionEngineService _promotionEngine;
    private readonly IPaymentService _paymentService;
    private readonly IAhamoveService _ahamove;
    private readonly AhamovePickupOptions _pickup;

    public CheckoutCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        PromotionEngineService promotionEngine,
        IPaymentService paymentService,
        IAhamoveService ahamove,
        IOptions<AhamovePickupOptions> pickup)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _promotionEngine = promotionEngine;
        _paymentService = paymentService;
        _ahamove = ahamove;
        _pickup = pickup.Value;
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

        // 8. Estimate shipping fee từ Ahamove
        var bulkyTier = AhamoveBulkyTierHelper.GetTierFromItems(
            cart.CartItems.Select(ci => (ci.Variant.WeightKg, ci.Variant.LengthCm, ci.Variant.WidthCm, ci.Variant.HeightCm, ci.Quantity))
        );
        if (bulkyTier.ExceedsLimit)
            throw new BadRequestException("Order is too heavy or too large for motorbike delivery. Please contact us for alternative shipping.");
        var shippingFee = await EstimateShippingFeeAsync(request, bulkyTier.Tier, cancellationToken);

        var totalAmount = baseAmount - rankDiscountAmount - couponDiscount + shippingFee;

        long orderId = 0;

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
                ShippingFee = shippingFee,
                TotalAmount = totalAmount,
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = PaymentStatus.UNPAID,
                Status = OrderStatus.PENDING,
                ShippingAddress = request.DeliveryAddress,
                ShippingServiceId = request.ServiceId,
                DeliveryLat = request.DeliveryLat,
                DeliveryLng = request.DeliveryLng,
                PaymentExpiredAt = paymentExpiredAt,
            };
            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();
            orderId = order.Id;

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

            // COD: tạo Shipment ngay sau khi commit
            if (request.PaymentMethod == PaymentMethod.COD)
            {
                await CreateShipmentAsync(orderId, request, totalAmount, user, bulkyTier.Tier, cancellationToken);
                return new CheckoutResult(orderId, PaymentLink: null);
            }

            // PayOS step 1: tạo PaymentTransaction PENDING trong transaction
            var pendingTransaction = new PaymentTransaction
            {
                OrderId = orderId,
                PayOSOrderCode = orderId,
                Amount = totalAmount,
                Status = PaymentTransactionStatus.PENDING,
            };
            await _unitOfWork.Orders.AddPaymentTransactionAsync(pendingTransaction);
            await _unitOfWork.SaveChangesAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        // PayOS step 2: gọi HTTP ra ngoài sau khi transaction đã commit
        try
        {
            var paymentLink = await _paymentService.CreatePaymentLinkAsync(orderId, totalAmount);

            var transaction = await _unitOfWork.Orders.GetPaymentTransactionByOrderIdAsync(orderId);
            if (transaction is not null)
            {
                transaction.CheckoutUrl = paymentLink;
                transaction.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Orders.UpdatePaymentTransaction(transaction);
                await _unitOfWork.SaveChangesAsync();
            }

            return new CheckoutResult(orderId, PaymentLink: paymentLink);
        }
        catch
        {
            throw new BadRequestException("Order created but failed to generate payment link. Please try again.");
        }
    }

    private async Task<decimal> EstimateShippingFeeAsync(CheckoutCommand request, string? bulkyTier, CancellationToken ct)
    {
        try
        {
            var bulkyRequests = bulkyTier is not null
                ? new List<AhamoveBulkyRequest> { new() { Id = $"{request.ServiceId}-BULKY", TierCode = bulkyTier } }
                : new List<AhamoveBulkyRequest>();

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
                Services = [new AhamoveEstimateService { Id = request.ServiceId, Requests = bulkyRequests }]
            };

            var estimates = await _ahamove.EstimateShippingFeeAsync(estimateRequest, ct);
            var match = estimates.FirstOrDefault(e => e.ServiceId == request.ServiceId);
            return match is not null ? (decimal)match.TotalPrice : 0;
        }
        catch
        {
            return 0;
        }
    }

    private async Task CreateShipmentAsync(long orderId, CheckoutCommand request, decimal totalAmount, User user, string? bulkyTier, CancellationToken ct)
    {
        try
        {
            var bulkyRequests = bulkyTier is not null
                ? [new AhamoveBulkyRequest { Id = $"{request.ServiceId}-BULKY", TierCode = bulkyTier }]
                : new List<AhamoveBulkyRequest>();

            var createRequest = new AhamoveCreateOrderRequest
            {
                ServiceId = request.ServiceId,
                PaymentMethod = "CASH",
                Requests = bulkyRequests,
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
                        Name = user.FullName ?? string.Empty,
                        Mobile = user.Phone ?? string.Empty,
                        Cod = (long)totalAmount,
                        TrackingNumber = orderId.ToString()
                    }
                ]
            };

            var response = await _ahamove.CreateOrderAsync(createRequest, ct);

            if (string.IsNullOrEmpty(response.Id))
                return;

            var shipment = new Shipment
            {
                OrderId = orderId,
                AhamoveOrderId = response.Id,
                Status = Domain.Enums.ShipmentStatus.ASSIGNING,
                ServiceId = response.ServiceId,
                TotalFee = response.TotalPay,
                CodAmount = totalAmount,
                SharedLink = response.SharedLink,
                PickupAddress = _pickup.Address,
                DeliveryAddress = request.DeliveryAddress,
                SupplierId = response.SupplierId,
                AhamoveCreateTime = response.OrderTime > 0
                    ? DateTimeOffset.FromUnixTimeSeconds((long)response.OrderTime).UtcDateTime
                    : null
            };

            await _unitOfWork.Shipments.AddAsync(shipment);
            await _unitOfWork.SaveChangesAsync();
        }
        catch
        {
            // silent — order is already committed
            throw new BadRequestException("Order created but failed to create shipment");
        }
    }
}
