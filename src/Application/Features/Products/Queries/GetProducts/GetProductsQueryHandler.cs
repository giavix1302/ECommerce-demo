using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Queries.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    internal const string CachePrefix = "products";

    public GetProductsQueryHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"{CachePrefix}:page={request.Page}:size={request.PageSize}:cat={request.CategoryId}";

        return await _cache.GetOrCreateAsync(cacheKey, CachePrefix, async () =>
        {
            var (items, totalCount) = await _unitOfWork.Products.GetPagedAsync(
                request.Page,
                request.PageSize,
                request.CategoryId);

            var dtos = items.Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Description,
                p.CategoryId,
                p.Category?.Name,
                p.Variants.FirstOrDefault(v => v.IsDefault)?.Price,
                p.Variants.FirstOrDefault(v => v.IsDefault)?.Sku
            ));

            return new PagedResult<ProductDto>(dtos, totalCount, request.Page, request.PageSize);
        }, TimeSpan.FromSeconds(60), cancellationToken) ?? new PagedResult<ProductDto>([], 0, request.Page, request.PageSize);
    }
}
