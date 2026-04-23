using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IProductAttributeRepository : IGenericRepository<ProductAttribute>
{
    Task<ProductAttribute?> GetByNameAsync(string name);
}
