using Application.Common.Interfaces;
using Domain.Enums;

namespace Application.Features.Payment.Jobs;

public class ExpiredPaymentJob
{
    private readonly IUnitOfWork _unitOfWork;

    public ExpiredPaymentJob(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync()
    {
        var expiredOrders = await _unitOfWork.Orders.GetExpiredPayOSOrdersAsync();

        foreach (var order in expiredOrders)
        {
            order.Status = OrderStatus.CANCELLED;
            order.PaymentStatus = PaymentStatus.FAILED;
            order.UpdatedAt = DateTime.UtcNow;

            foreach (var item in order.OrderItems)
            {
                item.Variant.StockQuantity += item.Quantity;
                _unitOfWork.Variants.Update(item.Variant);
            }

            foreach (var transaction in order.PaymentTransactions)
            {
                transaction.Status = PaymentTransactionStatus.FAILED;
                transaction.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Orders.UpdatePaymentTransaction(transaction);
            }

            _unitOfWork.Orders.Update(order);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
