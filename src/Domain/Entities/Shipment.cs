using Domain.Enums;

namespace Domain.Entities;

public class Shipment
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public string AhamoveOrderId { get; set; } = string.Empty;
    public ShipmentStatus Status { get; set; } = ShipmentStatus.ASSIGNING;
    public string? ServiceId { get; set; }
    public decimal TotalFee { get; set; }
    public decimal CodAmount { get; set; }
    public string? SharedLink { get; set; }
    public string PickupAddress { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? SupplierId { get; set; }
    public DateTime? AhamoveCreateTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Order Order { get; set; } = null!;
}
