using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PromotionRepository : GenericRepository<PromotionRule>, IPromotionRepository
{
    public PromotionRepository(ApplicationDbContext context) : base(context)
    {
    }
    public async Task<IEnumerable<PromotionRule>> GetActivePromotionRulesWithDetailsAsync()
    {
        var activePromotionRules = await _dbSet
            .Where(pr => pr.IsActive == true && (pr.StartDate <= DateTime.UtcNow) && (pr.EndDate >= DateTime.UtcNow))
            .Include(pr => pr.Conditions)
            .Include(pr => pr.Actions)
                .ThenInclude(a => a.GiftProduct)
                    .ThenInclude(p => p!.Variants.Where(v => v.IsDefault && !v.IsDeleted))
            .AsNoTracking()
            .ToListAsync();

        return activePromotionRules;
    }

    public async Task<(IEnumerable<PromotionRule> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
    {
        var query = _dbSet.AsNoTracking().OrderByDescending(pr => pr.Priority);
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<PromotionRule?> GetPromotionRuleWithDetailsAsync(long id)
    {
        var promotionRule = await _dbSet
            .Where(pr => pr.Id == id)
            .Include(pr => pr.Conditions)
            .Include(pr => pr.Actions)
            .FirstOrDefaultAsync();

        return promotionRule;
    }
}