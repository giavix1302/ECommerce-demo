using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<(IEnumerable<Order> Items, int TotalCount)> GetPagedByUserIdAsync(long userId, int page, int pageSize);
    Task<Order?> GetByIdWithItemsAsync(long orderId);
    Task<Order?> GetByIdWithItemsForUpdateAsync(long orderId);
    Task AddOrderItemAsync(OrderItem item);
    Task AddPaymentTransactionAsync(PaymentTransaction transaction);
    Task<PaymentTransaction?> GetPaymentTransactionByOrderCodeAsync(long orderCode);
    void UpdatePaymentTransaction(PaymentTransaction transaction);
    Task<PaymentTransaction?> GetPaymentTransactionByOrderIdAsync(long orderId);
    Task<IEnumerable<Order>> GetExpiredPayOSOrdersAsync();
}
