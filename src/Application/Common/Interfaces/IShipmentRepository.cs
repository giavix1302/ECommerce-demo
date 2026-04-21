using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IShipmentRepository
{
    Task AddAsync(Shipment shipment);
    Task<Shipment?> GetByAhamoveOrderIdAsync(string ahamoveOrderId);
    void Update(Shipment shipment);
}
