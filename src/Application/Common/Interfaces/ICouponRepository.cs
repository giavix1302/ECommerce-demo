using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICouponRepository : IGenericRepository<Coupon>
{
    Task<Coupon?> GetByCodeAsync(string code);
    Task<bool> HasUserUsedCouponAsync(long couponId, long userId);

    Task<(IEnumerable<Coupon> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);
}
