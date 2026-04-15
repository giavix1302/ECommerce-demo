using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Queries.GetProducts;

public record GetProductsQuery(
    int Page = 1,
    int PageSize = 10,
    long? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null
) : IRequest<PagedResult<ProductDto>>;

public record ProductDto(
    long Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    string? Sku,
    long? CategoryId,
    string? CategoryName
);
