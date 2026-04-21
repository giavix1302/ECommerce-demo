using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Constants;
using Domain.Enums;
using MediatR;

namespace Application.Features.Shipping.Commands.ProcessAhamoveWebhook;

public class ProcessAhamoveWebhookCommandHandler : IRequestHandler<ProcessAhamoveWebhookCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ProcessAhamoveWebhookCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ProcessAhamoveWebhookCommand request, CancellationToken cancellationToken)
    {
        var payload = request.Payload;

        var shipment = await _unitOfWork.Shipments.GetByAhamoveOrderIdAsync(payload.OrderId)
            ?? throw new NotFoundException("Shipment", payload.OrderId);

        // Ahamove sends top-level COMPLETED even when drop-off failed (path[1].status = "FAILED")
        var dropOffFailed = payload.Status == "COMPLETED"
            && payload.Path is { Count: > 1 }
            && payload.Path[1].Status == "FAILED";

        var newStatus = dropOffFailed ? ShipmentStatus.FAILED : payload.Status switch
        {
            "ASSIGNING" => ShipmentStatus.ASSIGNING,
            "ACCEPTED" => ShipmentStatus.ACCEPTED,
            "IN PROCESS" => ShipmentStatus.IN_PROCESS,
            "COMPLETED" => ShipmentStatus.COMPLETED,
            "CANCELLED" => ShipmentStatus.CANCELLED,
            "FAILED" => ShipmentStatus.FAILED,
            _ => shipment.Status
        };

        // Idempotency — cùng status gửi lại thì bỏ qua
        if (shipment.Status == newStatus)
            return;

        shipment.Status = newStatus;
        shipment.UpdatedAt = DateTime.UtcNow;

        if (payload.SupplierId is not null)
            shipment.SupplierId = payload.SupplierId;

        _unitOfWork.Shipments.Update(shipment);

        if (newStatus == ShipmentStatus.COMPLETED)
        {
            var order = shipment.Order;
            var user = shipment.Order.User;

            order.Status = OrderStatus.COMPLETED;
            order.UpdatedAt = DateTime.UtcNow;

            if (order.PaymentMethod == PaymentMethod.COD)
                order.PaymentStatus = PaymentStatus.PAID;

            var pointsEarned = (int)Math.Floor(order.TotalAmount / MembershipPolicy.PointsPerAmount);
            order.PointsEarned = pointsEarned;
            user.LoyaltyPoints += pointsEarned;

            user.MembershipRank = user.LoyaltyPoints switch
            {
                >= MembershipPolicy.DiamondThreshold => MembershipRank.DIAMOND,
                >= MembershipPolicy.GoldThreshold => MembershipRank.GOLD,
                _ => MembershipRank.SILVER
            };

            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Orders.Update(order);
            _unitOfWork.Users.Update(user);
        }
        else if (newStatus == ShipmentStatus.FAILED)
        {
            var order = shipment.Order;

            order.Status = OrderStatus.FAILED;
            order.UpdatedAt = DateTime.UtcNow;

            foreach (var item in order.OrderItems)
            {
                item.Variant.StockQuantity += item.Quantity;
                _unitOfWork.Variants.Update(item.Variant);
            }

            _unitOfWork.Orders.Update(order);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
