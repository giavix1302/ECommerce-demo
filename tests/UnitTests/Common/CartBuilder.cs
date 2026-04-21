using Domain.Entities;
using Domain.Enums;

namespace UnitTests.Common;

/// <summary>
/// Fluent builder for Cart test fixtures.
/// </summary>
internal class CartBuilder
{
    private readonly Cart _cart = new() { Id = 1, UserId = 1 };

    public CartBuilder WithItem(long variantId, decimal price, int qty, long productId = 1, long categoryId = 1)
    {
        var variant = new ProductVariant
        {
            Id = variantId,
            ProductId = productId,
            Price = price,
            StockQuantity = qty + 10,
            Product = new Product { Id = productId, CategoryId = categoryId }
        };
        _cart.CartItems.Add(new CartItem
        {
            Id = variantId,
            CartId = _cart.Id,
            VariantId = variantId,
            Quantity = qty,
            UnitPrice = price,
            Variant = variant
        });
        return this;
    }

    public CartBuilder WithCoupon(Coupon coupon, decimal precomputedDiscount = 0)
    {
        _cart.CouponId = coupon.Id;
        _cart.Coupon = coupon;
        _cart.DiscountAmount = precomputedDiscount;
        return this;
    }

    public Cart Build() => _cart;
}
