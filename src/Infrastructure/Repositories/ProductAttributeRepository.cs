using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ProductAttributeRepository : GenericRepository<ProductAttribute>, IProductAttributeRepository
{
    public ProductAttributeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ProductAttribute?> GetByNameAsync(string name)
        => await _dbSet.FirstOrDefaultAsync(a => a.Name == name);
}
