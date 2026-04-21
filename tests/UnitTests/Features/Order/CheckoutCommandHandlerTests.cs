using Application.Common.DTOs.Ahamove;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Services;
using Application.Features.Order.Commands.Checkout;
using Application.Features.Shipping.Queries.GetShippingFee;
using Domain.Constants;
using Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;
using UnitTests.Common;
using OrderEntity = Domain.Entities.Order;
using CouponEntity = Domain.Entities.Coupon;
using CartEntity = Domain.Entities.Cart;
using CartItemEntity = Domain.Entities.CartItem;
using ProductVariantEntity = Domain.Entities.ProductVariant;
using ProductEntity = Domain.Entities.Product;
using PaymentTransactionEntity = Domain.Entities.PaymentTransaction;
using UserEntity = Domain.Entities.User;
using PromotionRuleEntity = Domain.Entities.PromotionRule;

namespace UnitTests.Handlers.Order;

public class CheckoutCommandHandlerTests
{
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IPaymentService _paymentService = Substitute.For<IPaymentService>();
    private readonly IAhamoveService _ahamove = Substitute.For<IAhamoveService>();

    private CheckoutCommandHandler CreateHandler(IEnumerable<PromotionRuleEntity> rules)
    {
        _currentUser.UserId.Returns(1L);

        var cache = new MemoryCache(new MemoryCacheOptions());
        _uow.Promotions.GetActivePromotionRulesWithDetailsAsync()
            .Returns(Task.FromResult<IEnumerable<PromotionRuleEntity>>(rules));

        var promoEngine = new PromotionEngineService(_uow, cache);

        _ahamove.EstimateShippingFeeAsync(Arg.Any<AhamoveEstimateRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<AhamoveServiceEstimate>>([]));
        _ahamove.CreateOrderAsync(Arg.Any<AhamoveCreateOrderRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AhamoveOrderResponse { Id = string.Empty }));

        var pickup = Options.Create(new AhamovePickupOptions
        {
            Address = "123 Pickup", Lat = 10.0, Lng = 106.0, Name = "Shop", Mobile = "0901"
        });

        return new CheckoutCommandHandler(_uow, _currentUser, promoEngine, _paymentService, _ahamove, pickup);
    }

    private void SetupUser(MembershipRank rank = MembershipRank.SILVER)
    {
        _uow.Users.GetByIdAsync(1).Returns(new UserEntity
        {
            Id = 1,
            Email = "u@u.com",
            FullName = "User",
            Phone = "09000",
            MembershipRank = rank
        });
    }

    private void SetupTransactionInfra()
    {
        _uow.SaveChangesAsync().Returns(1);
        _uow.BeginTransactionAsync().Returns(Task.CompletedTask);
        _uow.CommitTransactionAsync().Returns(Task.CompletedTask);
        _uow.RollbackTransactionAsync().Returns(Task.CompletedTask);
        _uow.Orders.GetPaymentTransactionByOrderIdAsync(Arg.Any<long>())
            .Returns((PaymentTransactionEntity?)null);
    }

    private static CheckoutCommand CodCommand() =>
        new(PaymentMethod.COD, "123 Delivery St", 10.0, 106.0, "SGN-BIKE", null);

    // ── Empty cart ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_EmptyCart_ThrowsBadRequest()
    {
        _uow.Carts.GetByUserIdWithItemsAsync(1).Returns((CartEntity?)null);
        SetupUser();
        var handler = CreateHandler([]);

        var act = () => handler.Handle(CodCommand(), default);

        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*empty*");
    }

    // ── Stock validation ──────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_InsufficientStock_ThrowsBadRequest()
    {
        var variant = new ProductVariantEntity
        {
            Id = 1, ProductId = 1, StockQuantity = 1,
            Product = new ProductEntity { Id = 1, CategoryId = 1 }
        };
        var cart = new CartEntity { Id = 1, UserId = 1 };
        cart.CartItems.Add(new CartItemEntity
        {
            Id = 1, CartId = 1, VariantId = 1, Quantity = 5, UnitPrice = 100_000, Variant = variant
        });
        _uow.Carts.GetByUserIdWithItemsAsync(1).Returns(cart);
        SetupUser();
        var handler = CreateHandler([]);

        var act = () => handler.Handle(CodCommand(), default);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Insufficient stock*");
    }

    // ── Price formula: no promo, SILVER rank ──────────────────────────────────

    [Fact]
    public async Task Handle_CodNoPromoSilverRank_TotalEqualsSubtotal()
    {
        var cart = new CartBuilder().WithItem(1, 100_000, 2).Build(); // 200_000
        _uow.Carts.GetByUserIdWithItemsAsync(1).Returns(cart);
        SetupUser(MembershipRank.SILVER);
        SetupTransactionInfra();
        var handler = CreateHandler([]);

        OrderEntity? savedOrder = null;
        await _uow.Orders.AddAsync(Arg.Do<OrderEntity>(o => savedOrder = o));

        await handler.Handle(CodCommand(), default);

        savedOrder!.Subtotal.Should().Be(200_000);
        savedOrder.RankDiscountAmount.Should().Be(0);
        savedOrder.PromotionDiscountAmount.Should().Be(0);
        savedOrder.DiscountAmount.Should().Be(0);
        savedOrder.TotalAmount.Should().Be(200_000);
    }

    // ── Price formula: GOLD rank ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_GoldRank_RankDiscountAppliedOnBase()
    {
        var cart = new CartBuilder().WithItem(1, 200_000, 1).Build();
        _uow.Carts.GetByUserIdWithItemsAsync(1).Returns(cart);
        SetupUser(MembershipRank.GOLD);
        SetupTransactionInfra();
        var handler = CreateHandler([]);

        OrderEntity? savedOrder = null;
        await _uow.Orders.AddAsync(Arg.Do<OrderEntity>(o => savedOrder = o));

        await handler.Handle(CodCommand(), default);

        savedOrder!.RankDiscountAmount.Should().Be(200_000 * MembershipPolicy.GoldDiscount);
        savedOrder.TotalAmount.Should().Be(200_000 - 200_000 * MembershipPolicy.GoldDiscount);
    }

    // ── Price formula: promotion + rank ───────────────────────────────────────

    [Fact]
    public async Task Handle_PromotionAndGoldRank_DiscountOrderIsCorrect()
    {
        var promo = new PromotionBuilder().WithMinOrderValue(0).WithPercentageDiscount(10).Build();
        var cart = new CartBuilder().WithItem(1, 400_000, 1).Build();
        _uow.Carts.GetByUserIdWithItemsAsync(1).Returns(cart);
        SetupUser(MembershipRank.GOLD);
        SetupTransactionInfra();
        var handler = CreateHandler([promo]);

        OrderEntity? savedOrder = null;
        await _uow.Orders.AddAsync(Arg.Do<OrderEntity>(o => savedOrder = o));

        await handler.Handle(CodCommand(), default);

        // promo = 400_000 * 10% = 40_000; base = 360_000; rank = 18_000; total = 342_000
        savedOrder!.PromotionDiscountAmount.Should().Be(40_000);
        savedOrder.RankDiscountAmount.Should().Be(18_000);
        savedOrder.TotalAmount.Should().Be(342_000);
    }

    // ── Price formula: coupon (no promo) ─────────────────────────────────────

    [Fact]
    public async Task Handle_CouponNoPromo_CouponDiscountApplied()
    {
        var coupon = new CouponEntity
        {
            Id = 1, Code = "SAVE50",
            DiscountType = DiscountType.FIXED_AMOUNT, Value = 50_000,
            StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1)
        };
        var cart = new CartBuilder()
            .WithItem(1, 200_000, 1)
            .WithCoupon(coupon, precomputedDiscount: 50_000)
            .Build();
        _uow.Carts.GetByUserIdWithItemsAsync(1).Returns(cart);
        SetupUser(MembershipRank.SILVER);
        SetupTransactionInfra();
        var handler = CreateHandler([]);

        OrderEntity? savedOrder = null;
        await _uow.Orders.AddAsync(Arg.Do<OrderEntity>(o => savedOrder = o));

        await handler.Handle(CodCommand(), default);

        savedOrder!.DiscountAmount.Should().Be(50_000);
        savedOrder.TotalAmount.Should().Be(150_000);
    }

    // ── Price formula: coupon + promo AllowCoupon=true ────────────────────────

    [Fact]
    public async Task Handle_PromoAllowsCouponPercentage_CouponRecalculatedOnBase()
    {
        var promo = new PromotionBuilder()
            .WithMinOrderValue(0)
            .WithFixedDiscount(100_000)
            .Build();

        var coupon = new CouponEntity
        {
            Id = 1, Code = "PCT10",
            DiscountType = DiscountType.PERCENTAGE, Value = 10,
            StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1)
        };
        var cart = new CartBuilder()
            .WithItem(1, 500_000, 1)
            .WithCoupon(coupon, precomputedDiscount: 50_000)
            .Build();
        _uow.Carts.GetByUserIdWithItemsAsync(1).Returns(cart);
        SetupUser(MembershipRank.SILVER);
        SetupTransactionInfra();
        var handler = CreateHandler([promo]);

        OrderEntity? savedOrder = null;
        await _uow.Orders.AddAsync(Arg.Do<OrderEntity>(o => savedOrder = o));

        await handler.Handle(CodCommand(), default);

        // base = 500_000 - 100_000 = 400_000; coupon = 400_000 * 10% = 40_000; total = 360_000
        savedOrder!.DiscountAmount.Should().Be(40_000);
        savedOrder.TotalAmount.Should().Be(360_000);
    }

    // ── Price formula: coupon + promo AllowCoupon=false ───────────────────────

    [Fact]
    public async Task Handle_PromoDisallowsCoupon_CouponDiscountIsZero()
    {
        var promo = new PromotionBuilder()
            .WithMinOrderValue(0)
            .WithFixedDiscount(50_000)
            .DisallowCoupon()
            .Build();

        var coupon = new CouponEntity
        {
            Id = 1, Code = "SAVE30",
            DiscountType = DiscountType.FIXED_AMOUNT, Value = 30_000,
            StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1)
        };
        var cart = new CartBuilder()
            .WithItem(1, 200_000, 1)
            .WithCoupon(coupon, precomputedDiscount: 30_000)
            .Build();
        _uow.Carts.GetByUserIdWithItemsAsync(1).Returns(cart);
        SetupUser(MembershipRank.SILVER);
        SetupTransactionInfra();
        var handler = CreateHandler([promo]);

        OrderEntity? savedOrder = null;
        await _uow.Orders.AddAsync(Arg.Do<OrderEntity>(o => savedOrder = o));

        await handler.Handle(CodCommand(), default);

        savedOrder!.DiscountAmount.Should().Be(0);
        // base = 200_000 - 50_000 = 150_000; no coupon, no rank
        savedOrder.TotalAmount.Should().Be(150_000);
    }

    // ── GIVE_GIFT: missing SelectedGiftVariantId ──────────────────────────────

    [Fact]
    public async Task Handle_GiveGiftPromoNoSelectedVariant_ThrowsBadRequest()
    {
        var promo = new PromotionBuilder()
            .WithMinOrderValue(0)
            .WithGiftAction(giftProductId: 99)
            .Build();
        var cart = new CartBuilder().WithItem(1, 100_000, 1).Build();
        _uow.Carts.GetByUserIdWithItemsAsync(1).Returns(cart);
        SetupUser();
        var handler = CreateHandler([promo]);

        var cmd = new CheckoutCommand(PaymentMethod.COD, "Addr", 10.0, 106.0, "SGN-BIKE", null);
        var act = () => handler.Handle(cmd, default);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*SelectedGiftVariantId*");
    }

    // ── COD: returns null PaymentLink ─────────────────────────────────────────

    [Fact]
    public async Task Handle_CodPayment_ReturnsNullPaymentLink()
    {
        var cart = new CartBuilder().WithItem(1, 100_000, 1).Build();
        _uow.Carts.GetByUserIdWithItemsAsync(1).Returns(cart);
        SetupUser();
        SetupTransactionInfra();
        var handler = CreateHandler([]);

        var result = await handler.Handle(CodCommand(), default);

        result.PaymentLink.Should().BeNull();
    }

    // ── Stock is decremented ──────────────────────────────────────────────────

    [Fact]
    public async Task Handle_Checkout_DecrementsVariantStock()
    {
        var variant = new ProductVariantEntity
        {
            Id = 1, ProductId = 1, StockQuantity = 10,
            Product = new ProductEntity { Id = 1, CategoryId = 1 }
        };
        var cart = new CartEntity { Id = 1, UserId = 1 };
        cart.CartItems.Add(new CartItemEntity
        {
            Id = 1, CartId = 1, VariantId = 1, Quantity = 3, UnitPrice = 100_000, Variant = variant
        });
        _uow.Carts.GetByUserIdWithItemsAsync(1).Returns(cart);
        SetupUser();
        SetupTransactionInfra();
        var handler = CreateHandler([]);

        await handler.Handle(CodCommand(), default);

        variant.StockQuantity.Should().Be(7);
    }
}
