using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IVariantRepository : IGenericRepository<ProductVariant>
{
    Task<IEnumerable<ProductVariant>> GetByProductIdAsync(long productId);
    Task<ProductVariant?> GetByIdWithAttributesAsync(long id);
    Task<bool> ExistsBySkuAsync(string sku, long? excludeId = null);
}
