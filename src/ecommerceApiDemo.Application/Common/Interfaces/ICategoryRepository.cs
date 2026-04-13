using ecommerceApiDemo.Domain.Entities;

namespace ecommerceApiDemo.Application.Common.Interfaces;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug);
    Task<bool> ExistsBySlugAsync(string slug);
    Task<IEnumerable<Category>> GetWithChildrenAsync();
}
