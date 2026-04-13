using ecommerceApiDemo.Domain.Entities;

namespace ecommerceApiDemo.Application.Common.Interfaces;

public interface ICouponRepository : IGenericRepository<Coupon>
{
    Task<Coupon?> GetByCodeAsync(string code);
    Task<bool> HasUserUsedCouponAsync(long couponId, long userId);
}
