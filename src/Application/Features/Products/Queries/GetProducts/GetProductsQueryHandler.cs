using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Queries.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
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
            p.Category?.Name));

        return new PagedResult<ProductDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
