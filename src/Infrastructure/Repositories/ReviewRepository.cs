using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ReviewRepository : GenericRepository<Review>, IReviewRepository
{
    public ReviewRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<(IEnumerable<Review> Items, int TotalCount)> GetPagedByProductIdAsync(
        long productId,
        int page,
        int pageSize)
    {
        var query = _dbSet
            .Include(r => r.User)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<double> GetAverageRatingAsync(long productId)
    {
        var hasReviews = await _dbSet.AnyAsync(r => r.ProductId == productId);
        if (!hasReviews) return 0;

        return await _dbSet
            .Where(r => r.ProductId == productId)
            .AverageAsync(r => (double)r.Rating);
    }

    public async Task<bool> HasUserReviewedProductInOrderAsync(long userId, long productId, long orderId)
        => await _dbSet.AnyAsync(r =>
            r.UserId == userId &&
            r.ProductId == productId &&
            r.OrderId == orderId);

    public async Task<bool> HasCompletedOrderForProductAsync(long userId, long productId, long orderId)
        => await _context.Orders
            .Where(o => o.Id == orderId &&
                        o.UserId == userId &&
                        o.Status == Domain.Enums.OrderStatus.COMPLETED)
            .AnyAsync(o => o.OrderItems
                .Any(oi => oi.Variant.ProductId == productId));
}
