using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICartRepository : IGenericRepository<Cart>
{
    Task<Cart?> GetByUserIdAsync(long userId);
    Task<Cart?> GetByUserIdWithItemsAsync(long userId);
    void ClearItems(Cart cart);
}
