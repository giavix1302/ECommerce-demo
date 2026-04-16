using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetByIdWithVariantsAsync(long id)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Variants.Where(v => !v.IsDeleted))
                .ThenInclude(v => v.AttributeValues)
                .ThenInclude(av => av.Attribute)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
          int page,
          int pageSize,
          long? categoryId = null)
    {
        var query = _dbSet
            .Include(p => p.Category)
            .Include(p => p.Variants.Where(v => v.IsDefault && !v.IsDeleted))
            .Where(p => !p.IsDeleted);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
