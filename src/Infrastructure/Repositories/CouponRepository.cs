using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CouponRepository : GenericRepository<Coupon>, ICouponRepository
{
    public CouponRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Coupon?> GetByCodeAsync(string code)
        => await _dbSet.FirstOrDefaultAsync(c => c.Code == code);

    public async Task<bool> HasUserUsedCouponAsync(long couponId, long userId)
        => await _context.CouponUsages
            .AnyAsync(cu => cu.CouponId == couponId && cu.UserId == userId);
}
