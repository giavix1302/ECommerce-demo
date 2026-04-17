using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IPromotionRepository : IGenericRepository<PromotionRule>
{
    Task<PromotionRule?> GetPromotionRuleWithDetailsAsync(long id);
    Task<(IEnumerable<PromotionRule> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);
    Task<IEnumerable<PromotionRule>> GetActivePromotionRulesWithDetailsAsync();

}