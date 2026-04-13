using ecommerceApiDemo.Application.Common.Interfaces;
using ecommerceApiDemo.Infrastructure.Persistence;
using ecommerceApiDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ecommerceApiDemo.Infrastructure.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        long? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null)
    {
        var query = _dbSet
            .Include(p => p.Category)
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Product?> GetBySkuAsync(string sku)
        => await _dbSet.FirstOrDefaultAsync(p => p.Sku == sku && !p.IsDeleted);

    public async Task<bool> ExistsBySkuAsync(string sku)
        => await _dbSet.AnyAsync(p => p.Sku == sku && !p.IsDeleted);
}
