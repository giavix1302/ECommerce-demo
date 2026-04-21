using Application.Common.DTOs.Ahamove;
using Application.Common.Interfaces;
using Application.Common.Services;
using Application.Features.Order.Queries.GetCheckoutPreview;
using Application.Features.Shipping.Queries.GetShippingFee;
using Domain.Constants;
using Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;
using UnitTests.Common;
using CouponEntity = Domain.Entities.Coupon;
using UserEntity = Domain.Entities.User;
using PromotionRuleEntity = Domain.Entities.PromotionRule;
using CartEntity = Domain.Entities.Cart;

namespace UnitTests.Handlers.Order;

public class GetCheckoutPreviewQueryHandlerTests
{
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IAhamoveService _ahamove = Substitute.For<IAhamoveService>();

    private GetCheckoutPreviewQueryHandler CreateHandler(IEnumerable<PromotionRuleEntity> rules)
    {
        _currentUser.UserId.Returns(1L);

        var cache = new MemoryCache(new MemoryCacheOptions());
        _uow.Promotions.GetActivePromotionRulesWithDetailsAsync()
            .Returns(Task.FromResult<IEnumerable<PromotionRuleEntity>>(rules));

        var promoEngine = new PromotionEngineService(_uow, cache);

        // Default: ahamove returns empty (shipping not the focus here)
        _ahamove.EstimateShippingFeeAsync(Arg.Any<AhamoveEstimateRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<AhamoveServiceEstimate>>([]));

        var pickup = Options.Create(new AhamovePickupOptions
        {
            Address = "123 Pickup St",
            Lat = 10.0,
            Lng = 106.0,
            Name = "Shop",
            Mobile = "0901000000"
        });

        return new GetCheckoutPreviewQueryHandler(_uow, _currentUser, promoEngine, _ahamove, pickup);
    }

    private static GetCheckoutPreviewQuery MakeQuery() =>
        new("123 Delivery St", 10.0, 106.0);

    private void SetupUser(MembershipRank rank = MembershipRank.SILVER)
    {
        _uow.Users.GetByIdAsync(1).Returns(new UserEntity
        {
            Id = 1,
            Email = "test@test.com",
            MembershipRank = rank
        });
    }

    // ── Empty cart ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_EmptyCart_ThrowsBadRequest()
    {
        _uow.Carts.GetByUserIdWithItemsAsync(1).Returns((CartEntity?)null);
        SetupUser();
        var handler = CreateHandler([]);

        var act = () => handler.Handle(MakeQuery(), default);

        await act.Should().ThrowAsync<Application.Common.Exceptions.BadRequestException>();
    }

    // ── No promotion, no coupon ───────────────────────────────────────────────

    [Fact]
    public async Task Handle_NoPromoNoCoupon_TotalEqualsSubtotal()
    {
        var cart = new CartBuilder().WithItem(1, 100_000, 2).Build(); // 200_000
        _uow.Carts.GetByUserIdWithItemsAsync(1).Returns(cart);
        SetupUser(MembershipRank.SILVER);
        var handler = CreateHandler([]);

        var result = await handler.Handle(MakeQuery(), default);

        result.Subtotal.Should().Be(200_000);
        result.RankDiscountAmount.Should().Be(0); // SILVER = 0%
        result.Promotion.Should().BeNull();
        result.Coupon.Should().BeNull();
        result.TotalAmount.Should().Be(200_000);
    }

    // ── Rank discount ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_GoldRank_AppliesGoldDiscount()
    {
        var cart = new CartBuilder().WithItem(1, 200_000, 1).Build(); // 200_000
        _uow.Carts.GetByUserIdWithItemsAsync(1).Returns(cart);
        SetupUser(MembershipRank.GOLD);
        var handler = CreateHandler([]);

        var result = await handler.Handle(MakeQuery(), default);

        result.RankDiscountAmount.Should().Be(200_000 * MembershipPolicy.GoldDiscount); // 10_000
        result.TotalAmount.Should().Be(200_000 - 200_000 * MembershipPolicy.GoldDiscount);
    }

    [Fact]
    public async Task Handle_DiamondRank_AppliesDiamondDiscount()
    {
        var cart = new CartBuilder().WithItem(1, 200_000, 1).Build();
        _uow.Carts.GetByUserIdWithItemsAsync(1).Returns(cart);
        SetupUser(MembershipRank.DIAMOND);
        var handler = CreateHandler([]);

        var result = await handler.Handle(MakeQuery(), default);

        result.RankDiscountAmount.Should().Be(200_000 * MembershipPolicy.DiamondDiscount);
    }

    // ── Promotion discount ────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithPromotion_SubtractFromBaseBeforeRankDiscount()
    {
        // Promo: 10% off
        var promo = new PromotionBuilder().WithMinOrderValue(0).WithPercentageDiscount(10).Build();
        var cart = new CartBuilder().WithItem(1, 200_000, 1).Build(); // subtotal = 200_000
        _uow.Carts.GetByUserIdWithItemsAsync(1).Returns(cart);
        SetupUser(MembershipRank.GOLD); // 5%
        var handler = CreateHandler([promo]);

        var result = await handler.Handle(MakeQuery(), default);

        // base = 200_000 - 20_000 (promo 10%) = 180_000
        // rank = 180_000 * 5% = 9_000
        // total = 180_000 - 9_000 = 171_000
        result.Promotion!.PromotionDiscount.Should().Be(20_000);
        result.RankDiscountAmount.Should().Be(9_000);
        result.TotalAmount.Should().Be(171_000);
    }

    // ── Coupon: no promotion ──────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CouponNoPromotion_UsesPrecomputedDiscountAmount()
    {
        var coupon = new CouponEntity
        {
            Id = 1, Code = "SAVE10",
            DiscountType = DiscountType.FIXED_AMOUNT, Value = 30_000,
            StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1),
            IsActive = true
        };
        var cart = new CartBuilder()
            .WithItem(1, 200_000, 1)
            .WithCoupon(coupon, precomputedDiscount: 30_000) // pre-saved at ApplyCoupon time
            .Build();
        _uow.Carts.GetByUserIdWithItemsAsync(1).Returns(cart);
        SetupUser(MembershipRank.SILVER);
        var handler = CreateHandler([]);

        var result = await handler.Handle(MakeQuery(), default);

        result.Coupon!.DiscountAmount.Should().Be(30_000);
        result.TotalAmount.Should().Be(170_000); // 200_000 - 0 (rank) - 30_000 (coupon)
    }

    // ── Coupon: promotion AllowCoupon=false ───────────────────────────────────

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
            Id = 1, Code = "SAVE10",
            DiscountType = DiscountType.FIXED_AMOUNT, Value = 30_000,
            StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1),
            IsActive = true
        };
        var cart = new CartBuilder()
            .WithItem(1, 200_000, 1)
            .WithCoupon(coupon, precomputedDiscount: 30_000)
            .Build();
        _uow.Carts.GetByUserIdWithItemsAsync(1).Returns(cart);
        SetupUser(MembershipRank.SILVER);
        var handler = CreateHandler([promo]);

        var result = await handler.Handle(MakeQuery(), default);

        result.Coupon!.DiscountAmount.Should().Be(0); // blocked by promotion
        result.Promotion!.AllowCoupon.Should().BeFalse();
    }

    // ── Coupon: promotion AllowCoupon=true → recalculate on base ─────────────

    [Fact]
    public async Task Handle_PromoAllowsCoupon_CouponRecalculatedOnBase()
    {
        var promo = new PromotionBuilder()
            .WithMinOrderValue(0)
            .WithFixedDiscount(50_000)
            .Build(); // AllowCoupon = true by default

        var coupon = new CouponEntity
        {
            Id = 1, Code = "PCNT10",
            DiscountType = DiscountType.PERCENTAGE, Value = 10,
            StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1),
            IsActive = true
        };
        var cart = new CartBuilder()
            .WithItem(1, 400_000, 1)
            .WithCoupon(coupon, precomputedDiscount: 40_000) // original precomputed
            .Build();
        _uow.Carts.GetByUserIdWithItemsAsync(1).Returns(cart);
        SetupUser(MembershipRank.SILVER);
        var handler = CreateHandler([promo]);

        var result = await handler.Handle(MakeQuery(), default);

        // base = 400_000 - 50_000 = 350_000
        // coupon recalculated = 350_000 * 10% = 35_000
        result.Coupon!.DiscountAmount.Should().Be(35_000);
        result.TotalAmount.Should().Be(315_000); // 350_000 - 35_000
    }
}
