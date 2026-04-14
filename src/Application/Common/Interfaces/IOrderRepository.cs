using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<IEnumerable<Order>> GetByUserIdAsync(long userId);
    Task<Order?> GetByIdWithItemsAsync(long orderId);
}
