using ecommerceApiDemo.Domain.Entities;

namespace ecommerceApiDemo.Application.Common.Interfaces;

public interface ICartRepository : IGenericRepository<Cart>
{
    Task<Cart?> GetByUserIdAsync(long userId);
    Task<Cart?> GetByUserIdWithItemsAsync(long userId);
}
