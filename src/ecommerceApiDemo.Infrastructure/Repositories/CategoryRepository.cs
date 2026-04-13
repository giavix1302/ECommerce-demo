using ecommerceApiDemo.Application.Common.Interfaces;
using ecommerceApiDemo.Infrastructure.Persistence;
using ecommerceApiDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ecommerceApiDemo.Infrastructure.Repositories;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Category?> GetBySlugAsync(string slug)
        => await _dbSet.FirstOrDefaultAsync(c => c.Slug == slug);

    public async Task<bool> ExistsBySlugAsync(string slug)
        => await _dbSet.AnyAsync(c => c.Slug == slug);

    public async Task<IEnumerable<Category>> GetWithChildrenAsync()
        => await _dbSet
            .Include(c => c.Children)
            .Where(c => c.ParentId == null)
            .ToListAsync();
}
