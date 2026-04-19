using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<(IEnumerable<Order> Items, int TotalCount)> GetPagedByUserIdAsync(long userId, int page, int pageSize);
    Task<Order?> GetByIdWithItemsAsync(long orderId);
    Task AddOrderItemAsync(OrderItem item);
    Task AddPaymentTransactionAsync(PaymentTransaction transaction);
}
