using ecommerceApiDemo.Domain.Entities;

namespace ecommerceApiDemo.Application.Common.Interfaces;

public interface IReviewRepository : IGenericRepository<Review>
{
    Task<(IEnumerable<Review> Items, int TotalCount)> GetPagedByProductIdAsync(
        long productId,
        int page,
        int pageSize);

    Task<double> GetAverageRatingAsync(long productId);
    Task<bool> HasUserReviewedProductInOrderAsync(long userId, long productId, long orderId);
}
