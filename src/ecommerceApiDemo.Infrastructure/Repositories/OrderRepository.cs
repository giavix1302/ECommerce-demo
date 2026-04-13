using ecommerceApiDemo.Application.Common.Interfaces;
using ecommerceApiDemo.Infrastructure.Persistence;
using ecommerceApiDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ecommerceApiDemo.Infrastructure.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(long userId)
        => await _dbSet
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Coupon)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<Order?> GetByIdWithItemsAsync(long orderId)
        => await _dbSet
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Coupon)
            .FirstOrDefaultAsync(o => o.Id == orderId);
}
