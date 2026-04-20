using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;

namespace Application.Features.Payment.Commands.ProcessPayOSWebhook;

public class ProcessPayOSWebhookCommandHandler : IRequestHandler<ProcessPayOSWebhookCommand>
{

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentService _paymentService;

    public ProcessPayOSWebhookCommandHandler(IUnitOfWork unitOfWork, IPaymentService paymentService)
    {
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
    }

    public async Task Handle(ProcessPayOSWebhookCommand request, CancellationToken cancellationToken)
    {
        var payload = request.Payload;

        var orderCode = await _paymentService.VerifyWebhookAsync(payload);

        var transaction = await _unitOfWork.Orders.GetPaymentTransactionByOrderCodeAsync(orderCode)
            ?? throw new NotFoundException("PaymentTransaction", orderCode);

        var order = await _unitOfWork.Orders.GetByIdWithItemsForUpdateAsync(transaction.OrderId)
            ?? throw new NotFoundException("Order", transaction.OrderId);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (payload.Code == "00" && payload.Success)
            {
                transaction.Status = PaymentTransactionStatus.PAID;
                transaction.AccountNumber = payload.Data.AccountNumber;
                transaction.Reference = payload.Data.Reference;
                transaction.TransactionDateTime = DateTime.TryParse(payload.Data.TransactionDateTime, out var dt) ? dt : null;
                transaction.Currency = payload.Data.Currency;
                transaction.PaymentLinkId = payload.Data.PaymentLinkId;
                transaction.CounterAccountBankName = payload.Data.CounterAccountBankName;
                transaction.CounterAccountName = payload.Data.CounterAccountName;
                transaction.CounterAccountNumber = payload.Data.CounterAccountNumber;
                transaction.UpdatedAt = DateTime.UtcNow;

                order.PaymentStatus = PaymentStatus.PAID;
                order.UpdatedAt = DateTime.UtcNow;

                // TODO: Task 8 — gọi IAhamoveService tạo Shipment
            }
            else
            {
                transaction.Status = PaymentTransactionStatus.FAILED;
                transaction.UpdatedAt = DateTime.UtcNow;

                order.Status = OrderStatus.CANCELLED;
                order.PaymentStatus = PaymentStatus.FAILED;
                order.UpdatedAt = DateTime.UtcNow;

                foreach (var item in order.OrderItems)
                {
                    item.Variant.StockQuantity += item.Quantity;
                    _unitOfWork.Variants.Update(item.Variant);
                }
            }

            _unitOfWork.Orders.Update(order);
            _unitOfWork.Orders.UpdatePaymentTransaction(transaction);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
