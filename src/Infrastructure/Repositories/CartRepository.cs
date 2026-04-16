using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CartRepository : GenericRepository<Cart>, ICartRepository
{
    public CartRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Cart?> GetByUserIdAsync(long userId)
        => await _dbSet.FirstOrDefaultAsync(c => c.UserId == userId);

    public async Task<Cart?> GetByUserIdWithItemsAsync(long userId)
        => await _dbSet
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Variant)
                .ThenInclude(v => v.Product)

            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Variant)
                .ThenInclude(v => v.AttributeValues)
                .ThenInclude(av => av.Attribute)
            .Include(c => c.Coupon)
            .FirstOrDefaultAsync(c => c.UserId == userId);
}
