using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class VariantAttributeValueRepository : GenericRepository<VariantAttributeValue>, IVariantAttributeValueRepository
{
    public VariantAttributeValueRepository(ApplicationDbContext context) : base(context)
    {
    }
}
