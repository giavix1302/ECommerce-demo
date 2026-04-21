using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Categories.Queries.GetCategories;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IEnumerable<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    internal const string CachePrefix = "categories";
    private const string CacheKey = "categories:all";

    public GetCategoriesQueryHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<IEnumerable<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _cache.GetOrCreateAsync(CacheKey, CachePrefix, async () =>
        {
            var categories = await _unitOfWork.Categories.GetWithChildrenAsync();
            return categories.Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Slug,
                c.ParentId,
                c.Parent?.Name,
                c.Children.Select(ch => new CategoryChildDto(ch.Id, ch.Name, ch.Slug))
            ));
        }, TimeSpan.FromSeconds(60), cancellationToken) ?? [];
    }
}
