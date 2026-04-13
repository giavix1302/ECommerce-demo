using ecommerceApiDemo.Domain.Entities;

namespace ecommerceApiDemo.Application.Common.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        long? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null);

    Task<Product?> GetBySkuAsync(string sku);
    Task<bool> ExistsBySkuAsync(string sku);
}
