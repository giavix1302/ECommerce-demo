using Application.Common.Interfaces;
using Application.Common.Services;
using Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using UnitTests.Common;

namespace UnitTests.Features.PromotionEngine;

public class PromotionEngineServiceTests
{
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

    private PromotionEngineService CreateService(IEnumerable<PromotionRule> rules)
    {
        _uow.Promotions.GetActivePromotionRulesWithDetailsAsync()
            .Returns(Task.FromResult<IEnumerable<PromotionRule>>(rules));
        return new PromotionEngineService(_uow, _cache);
    }

    // ── No rules ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task EvaluateAsync_NoRules_ReturnsNullAndZero()
    {
        var svc = CreateService([]);
        var cart = new CartBuilder().WithItem(1, 100_000, 2).Build();

        var (rule, discount) = await svc.EvaluateAsync(cart);

        rule.Should().BeNull();
        discount.Should().Be(0);
    }

    // ── MIN_ORDER_VALUE condition ──────────────────────────────────────────────

    [Fact]
    public async Task EvaluateAsync_CartBelowMinOrderValue_RuleNotApplied()
    {
        var promo = new PromotionBuilder()
            .WithMinOrderValue(500_000)
            .WithPercentageDiscount(10)
            .Build();
        var svc = CreateService([promo]);
        var cart = new CartBuilder().WithItem(1, 100_000, 2).Build(); // subtotal = 200_000

        var (rule, _) = await svc.EvaluateAsync(cart);

        rule.Should().BeNull();
    }

    [Fact]
    public async Task EvaluateAsync_CartMeetsMinOrderValue_RuleApplied()
    {
        var promo = new PromotionBuilder()
            .WithMinOrderValue(200_000)
            .WithPercentageDiscount(10)
            .Build();
        var svc = CreateService([promo]);
        var cart = new CartBuilder().WithItem(1, 100_000, 2).Build(); // subtotal = 200_000

        var (rule, discount) = await svc.EvaluateAsync(cart);

        rule.Should().NotBeNull();
        discount.Should().Be(20_000); // 200_000 * 10%
    }

    // ── PERCENTAGE_DISCOUNT calculation ───────────────────────────────────────

    [Fact]
    public async Task EvaluateAsync_PercentageDiscount_CalculatesCorrectly()
    {
        var promo = new PromotionBuilder()
            .WithMinOrderValue(0)
            .WithPercentageDiscount(15)
            .Build();
        var svc = CreateService([promo]);
        var cart = new CartBuilder().WithItem(1, 200_000, 3).Build(); // subtotal = 600_000

        var (_, discount) = await svc.EvaluateAsync(cart);

        discount.Should().Be(90_000); // 600_000 * 15%
    }

    // ── FIXED_DISCOUNT calculation ─────────────────────────────────────────────

    [Fact]
    public async Task EvaluateAsync_FixedDiscount_CalculatesCorrectly()
    {
        var promo = new PromotionBuilder()
            .WithMinOrderValue(0)
            .WithFixedDiscount(50_000)
            .Build();
        var svc = CreateService([promo]);
        var cart = new CartBuilder().WithItem(1, 100_000, 2).Build();

        var (_, discount) = await svc.EvaluateAsync(cart);

        discount.Should().Be(50_000);
    }

    // ── GIVE_GIFT action ───────────────────────────────────────────────────────

    [Fact]
    public async Task EvaluateAsync_GiveGift_DiscountEqualsGiftPrice()
    {
        var promo = new PromotionBuilder()
            .WithMinOrderValue(0)
            .WithGiftAction(giftProductId: 99, giftQty: 2, giftPrice: 50_000)
            .Build();
        var svc = CreateService([promo]);
        var cart = new CartBuilder().WithItem(1, 100_000, 1).Build();

        var (_, discount) = await svc.EvaluateAsync(cart);

        discount.Should().Be(100_000); // 50_000 * 2
    }

    // ── MIN_QUANTITY condition ─────────────────────────────────────────────────

    [Fact]
    public async Task EvaluateAsync_MinQuantityNotMet_RuleNotApplied()
    {
        var promo = new PromotionBuilder()
            .WithMinQuantity(5)
            .WithPercentageDiscount(10)
            .Build();
        var svc = CreateService([promo]);
        var cart = new CartBuilder().WithItem(1, 100_000, 3).Build(); // qty = 3 < 5

        var (rule, _) = await svc.EvaluateAsync(cart);

        rule.Should().BeNull();
    }

    [Fact]
    public async Task EvaluateAsync_MinQuantityWithProductTarget_OnlyCountsTargetProduct()
    {
        const long targetProductId = 10;
        var promo = new PromotionBuilder()
            .WithMinQuantity(3, productId: targetProductId)
            .WithPercentageDiscount(10)
            .Build();
        var svc = CreateService([promo]);

        // item1: productId=10, qty=2 ; item2: productId=20, qty=5
        var cart = new CartBuilder()
            .WithItem(1, 100_000, 2, productId: targetProductId)
            .WithItem(2, 100_000, 5, productId: 20)
            .Build();

        var (rule, _) = await svc.EvaluateAsync(cart);

        // only 2 units of target product → condition not met
        rule.Should().BeNull();
    }

    // ── Best rule selection: higher discount wins ──────────────────────────────

    [Fact]
    public async Task EvaluateAsync_MultipleRules_PicksHighestDiscount()
    {
        var promo10 = new PromotionBuilder(id: 1).WithMinOrderValue(0).WithPercentageDiscount(10).Build();
        var promo20 = new PromotionBuilder(id: 2).WithMinOrderValue(0).WithPercentageDiscount(20).Build();
        var svc = CreateService([promo10, promo20]);
        var cart = new CartBuilder().WithItem(1, 100_000, 1).Build();

        var (rule, discount) = await svc.EvaluateAsync(cart);

        rule!.Id.Should().Be(2);
        discount.Should().Be(20_000);
    }

    // ── Tie-break: equal discount → higher Priority wins ──────────────────────

    [Fact]
    public async Task EvaluateAsync_TieDiscount_PicksHigherPriority()
    {
        // Both fixed 50_000 discount, promo2 has higher priority
        var promo1 = new PromotionBuilder(id: 1, priority: 1).WithMinOrderValue(0).WithFixedDiscount(50_000).Build();
        var promo2 = new PromotionBuilder(id: 2, priority: 5).WithMinOrderValue(0).WithFixedDiscount(50_000).Build();
        var svc = CreateService([promo1, promo2]);
        var cart = new CartBuilder().WithItem(1, 100_000, 1).Build();

        var (rule, _) = await svc.EvaluateAsync(cart);

        rule!.Id.Should().Be(2);
    }

    // ── Cache invalidation ────────────────────────────────────────────────────

    [Fact]
    public async Task EvaluateAsync_AfterInvalidate_ReloadsRulesFromRepo()
    {
        var promo = new PromotionBuilder(id: 1).WithMinOrderValue(0).WithFixedDiscount(10_000).Build();
        var svc = CreateService([promo]);
        var cart = new CartBuilder().WithItem(1, 100_000, 1).Build();

        await svc.EvaluateAsync(cart); // populate cache

        // Update repo to return no rules
        _uow.Promotions.GetActivePromotionRulesWithDetailsAsync()
            .Returns(Task.FromResult<IEnumerable<PromotionRule>>([]));
        svc.InvalidateCache();

        var (rule, _) = await svc.EvaluateAsync(cart);

        rule.Should().BeNull();
    }
}
