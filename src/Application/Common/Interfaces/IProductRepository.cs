using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        long? categoryId = null);

    Task<Product?> GetByIdWithVariantsAsync(long id);
}
