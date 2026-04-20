using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }
    public async Task<Order?> GetByIdWithItemsAsync(long orderId)
        => await _dbSet
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Variant)
                .ThenInclude(v => v.Product)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Variant)
                .ThenInclude(v => v.AttributeValues)
                .ThenInclude(av => av.Attribute)
            .Include(o => o.Coupon)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId);

    public async Task<Order?> GetByIdWithItemsForUpdateAsync(long orderId)
        => await _dbSet
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Variant)
            .FirstOrDefaultAsync(o => o.Id == orderId);

    public async Task<PaymentTransaction?> GetPaymentTransactionByOrderCodeAsync(long orderCode)
        => await _context.PaymentTransactions
            .FirstOrDefaultAsync(pt => pt.PayOSOrderCode == orderCode);

    public void UpdatePaymentTransaction(PaymentTransaction transaction)
        => _context.PaymentTransactions.Update(transaction);

    public async Task<PaymentTransaction?> GetPaymentTransactionByOrderIdAsync(long orderId)
        => await _context.PaymentTransactions
            .FirstOrDefaultAsync(pt => pt.OrderId == orderId);

    public async Task<IEnumerable<Order>> GetExpiredPayOSOrdersAsync()
        => await _dbSet
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Variant)
            .Include(o => o.PaymentTransactions)
            .Where(o => o.PaymentMethod == Domain.Enums.PaymentMethod.PAYOS
                     && o.PaymentStatus == Domain.Enums.PaymentStatus.UNPAID
                     && o.PaymentExpiredAt < DateTime.UtcNow)
            .ToListAsync();

    public async Task AddOrderItemAsync(OrderItem item)
        => await _context.OrderItems.AddAsync(item);

    public async Task AddPaymentTransactionAsync(PaymentTransaction transaction)
        => await _context.PaymentTransactions.AddAsync(transaction);

    public async Task<(IEnumerable<Order> Items, int TotalCount)> GetPagedByUserIdAsync(long userId, int page, int pageSize)
    {
        var query = _dbSet.Where(o => o.UserId == userId).Include(o => o.OrderItems).AsNoTracking().OrderByDescending(o => o.CreatedAt);
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
