using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ShipmentRepository : GenericRepository<Shipment>, IShipmentRepository
{
    public ShipmentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Shipment?> GetByAhamoveOrderIdAsync(string ahamoveOrderId)
        => await _dbSet
            .Include(s => s.Order)
                .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.Variant)
            .Include(s => s.Order)
                .ThenInclude(o => o.User)
            .FirstOrDefaultAsync(s => s.AhamoveOrderId == ahamoveOrderId);
}
