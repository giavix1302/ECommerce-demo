using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class VariantRepository : GenericRepository<ProductVariant>, IVariantRepository
{
    public VariantRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProductVariant>> GetByProductIdAsync(long productId)
        => await _dbSet
            .Include(v => v.AttributeValues)
                .ThenInclude(av => av.Attribute)
            .Where(v => v.ProductId == productId && !v.IsDeleted)
            .OrderBy(v => v.Id)
            .ToListAsync();

    public async Task<ProductVariant?> GetByIdWithAttributesAsync(long id)
        => await _dbSet
            .Include(v => v.AttributeValues)
                .ThenInclude(av => av.Attribute)
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);

    public async Task<bool> ExistsBySkuAsync(string sku, long? excludeId = null)
        => await _dbSet.AnyAsync(v =>
            v.Sku == sku &&
            !v.IsDeleted &&
            (excludeId == null || v.Id != excludeId.Value));
}
