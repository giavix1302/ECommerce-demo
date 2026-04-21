using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Coupon.Commands.ApplyCoupon;
using Domain.Enums;
using FluentAssertions;
using NSubstitute;
using UnitTests.Common;
using CouponEntity = Domain.Entities.Coupon;

namespace UnitTests.Features.Coupon;

public class ApplyCouponCommandHandlerTests
{
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly ApplyCouponCommandHandler _handler;

    public ApplyCouponCommandHandlerTests()
    {
        _currentUser.UserId.Returns(1L);
        _handler = new ApplyCouponCommandHandler(_uow, _currentUser);
    }

    private CouponEntity MakeActiveCoupon(
        DiscountType type = DiscountType.FIXED_AMOUNT,
        decimal value = 50_000,
        decimal minOrderValue = 0,
        int? usageLimit = null,
        int usedCount = 0)
    {
        return new CouponEntity
        {
            Id = 1,
            Code = "TEST10",
            IsActive = true,
            DiscountType = type,
            Value = value,
            MinOrderValue = minOrderValue,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            UsageLimit = usageLimit,
            UsedCount = usedCount
        };
    }

    private void SetupCouponAndCart(CouponEntity coupon, Domain.Entities.Cart cart, bool alreadyUsed = false)
    {
        _uow.Coupons.GetByCodeAsync(coupon.Code).Returns(coupon);
        _uow.Carts.GetByUserIdWithItemsAsync(1).Returns(cart);
        _uow.Coupons.HasUserUsedCouponAsync(coupon.Id, 1).Returns(alreadyUsed);
        _uow.SaveChangesAsync().Returns(1);
    }

    // ── Not found ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CouponNotFound_ThrowsNotFoundException()
    {
        _uow.Coupons.GetByCodeAsync("INVALID").Returns((CouponEntity?)null);

        var act = () => _handler.Handle(new ApplyCouponCommand("INVALID"), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── Inactive / expired ───────────────────────────────────────────────────

    [Fact]
    public async Task Handle_InactiveCoupon_ThrowsBadRequest()
    {
        var coupon = MakeActiveCoupon();
        coupon.IsActive = false;
        _uow.Coupons.GetByCodeAsync(coupon.Code).Returns(coupon);

        var act = () => _handler.Handle(new ApplyCouponCommand(coupon.Code), default);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*not active*");
    }

    [Fact]
    public async Task Handle_ExpiredCoupon_ThrowsBadRequest()
    {
        var coupon = MakeActiveCoupon();
        coupon.EndDate = DateTime.UtcNow.AddDays(-1);
        _uow.Coupons.GetByCodeAsync(coupon.Code).Returns(coupon);

        var act = () => _handler.Handle(new ApplyCouponCommand(coupon.Code), default);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    // ── Usage limit ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_UsageLimitReached_ThrowsBadRequest()
    {
        var coupon = MakeActiveCoupon(usageLimit: 10, usedCount: 10);
        _uow.Coupons.GetByCodeAsync(coupon.Code).Returns(coupon);

        var act = () => _handler.Handle(new ApplyCouponCommand(coupon.Code), default);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*usage limit*");
    }

    [Fact]
    public async Task Handle_UsageLimitNull_UnlimitedUsage_Allowed()
    {
        var coupon = MakeActiveCoupon(usageLimit: null, usedCount: 999);
        var cart = new CartBuilder().WithItem(1, 100_000, 1).Build();
        SetupCouponAndCart(coupon, cart);

        var result = await _handler.Handle(new ApplyCouponCommand(coupon.Code), default);

        result.Should().NotBeNull();
    }

    // ── Empty cart ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_EmptyCart_ThrowsBadRequest()
    {
        var coupon = MakeActiveCoupon();
        _uow.Coupons.GetByCodeAsync(coupon.Code).Returns(coupon);
        _uow.Carts.GetByUserIdWithItemsAsync(1).Returns((Domain.Entities.Cart?)null);

        var act = () => _handler.Handle(new ApplyCouponCommand(coupon.Code), default);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*empty*");
    }

    // ── MinOrderValue ────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CartBelowMinOrderValue_ThrowsBadRequest()
    {
        var coupon = MakeActiveCoupon(minOrderValue: 500_000);
        var cart = new CartBuilder().WithItem(1, 100_000, 2).Build(); // 200_000 < 500_000
        SetupCouponAndCart(coupon, cart);

        var act = () => _handler.Handle(new ApplyCouponCommand(coupon.Code), default);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Minimum order value*");
    }

    // ── Already used ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_UserAlreadyUsedCoupon_ThrowsBadRequest()
    {
        var coupon = MakeActiveCoupon();
        var cart = new CartBuilder().WithItem(1, 100_000, 2).Build();
        SetupCouponAndCart(coupon, cart, alreadyUsed: true);

        var act = () => _handler.Handle(new ApplyCouponCommand(coupon.Code), default);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*already used*");
    }

    // ── Discount calculation ─────────────────────────────────────────────────

    [Fact]
    public async Task Handle_FixedAmountCoupon_ReturnsCorrectDiscount()
    {
        var coupon = MakeActiveCoupon(DiscountType.FIXED_AMOUNT, value: 30_000);
        var cart = new CartBuilder().WithItem(1, 200_000, 1).Build();
        SetupCouponAndCart(coupon, cart);

        var result = await _handler.Handle(new ApplyCouponCommand(coupon.Code), default);

        result.DiscountAmount.Should().Be(30_000);
    }

    [Fact]
    public async Task Handle_PercentageCoupon_ReturnsCorrectDiscount()
    {
        var coupon = MakeActiveCoupon(DiscountType.PERCENTAGE, value: 10); // 10%
        var cart = new CartBuilder().WithItem(1, 200_000, 2).Build(); // subtotal = 400_000
        SetupCouponAndCart(coupon, cart);

        var result = await _handler.Handle(new ApplyCouponCommand(coupon.Code), default);

        result.DiscountAmount.Should().Be(40_000); // 400_000 * 10%
    }

    // ── Happy path: cart is updated ──────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidCoupon_UpdatesCartAndSaves()
    {
        var coupon = MakeActiveCoupon(DiscountType.FIXED_AMOUNT, value: 50_000);
        var cart = new CartBuilder().WithItem(1, 100_000, 1).Build();
        SetupCouponAndCart(coupon, cart);

        await _handler.Handle(new ApplyCouponCommand(coupon.Code), default);

        cart.CouponId.Should().Be(coupon.Id);
        cart.DiscountAmount.Should().Be(50_000);
        await _uow.Received(1).SaveChangesAsync();
    }
}
