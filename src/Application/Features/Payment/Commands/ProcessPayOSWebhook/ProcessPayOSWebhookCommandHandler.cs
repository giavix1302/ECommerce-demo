using Application.Common.DTOs.Ahamove;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Shipping.Queries.GetShippingFee;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Options;

namespace Application.Features.Payment.Commands.ProcessPayOSWebhook;

public class ProcessPayOSWebhookCommandHandler : IRequestHandler<ProcessPayOSWebhookCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentService _paymentService;
    private readonly IAhamoveService _ahamove;
    private readonly AhamovePickupOptions _pickup;

    public ProcessPayOSWebhookCommandHandler(
        IUnitOfWork unitOfWork,
        IPaymentService paymentService,
        IAhamoveService ahamove,
        IOptions<AhamovePickupOptions> pickup)
    {
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
        _ahamove = ahamove;
        _pickup = pickup.Value;
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

        // Tạo Shipment sau khi commit (PayOS đã PAID)
        if (payload.Code == "00" && payload.Success)
        {
            await CreateShipmentAsync(order, cancellationToken);
        }
    }

    private async Task CreateShipmentAsync(Domain.Entities.Order order, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(order.ShippingAddress) || string.IsNullOrEmpty(order.ShippingServiceId))
            return;

        try
        {
            var user = order.User;
            var createRequest = new AhamoveCreateOrderRequest
            {
                ServiceId = order.ShippingServiceId,
                PaymentMethod = "CASH",
                Path =
                [
                    new AhamoveOrderPath
                    {
                        Lat = _pickup.Lat,
                        Lng = _pickup.Lng,
                        Address = _pickup.Address,
                        Name = _pickup.Name,
                        Mobile = _pickup.Mobile
                    },
                    new AhamoveOrderPath
                    {
                        Lat = order.DeliveryLat ?? 0,
                        Lng = order.DeliveryLng ?? 0,
                        Address = order.ShippingAddress,
                        Name = user.FullName ?? string.Empty,
                        Mobile = user.Phone ?? string.Empty,
                        Cod = 0,
                        TrackingNumber = order.Id.ToString()
                    }
                ]
            };

            var response = await _ahamove.CreateOrderAsync(createRequest, ct);

            if (string.IsNullOrEmpty(response.Id))
                return;

            var shipment = new Shipment
            {
                OrderId = order.Id,
                AhamoveOrderId = response.Id,
                Status = Domain.Enums.ShipmentStatus.ASSIGNING,
                ServiceId = response.ServiceId,
                TotalFee = response.TotalPay,
                CodAmount = 0,
                SharedLink = response.SharedLink,
                PickupAddress = _pickup.Address,
                DeliveryAddress = order.ShippingAddress,
                SupplierId = response.SupplierId,
                AhamoveCreateTime = response.OrderTime > 0
                    ? DateTimeOffset.FromUnixTimeSeconds((long)response.OrderTime).UtcDateTime
                    : null
            };

            await _unitOfWork.Shipments.AddAsync(shipment);
            await _unitOfWork.SaveChangesAsync();
        }
        catch
        {
            // Shipment creation failed — order vẫn PAID, admin có thể tạo lại thủ công
        }
    }
}
