using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Queries.GetProducts;

public record GetProductsQuery(
    int Page = 1,
    int PageSize = 10,
    long? CategoryId = null
) : IRequest<PagedResult<ProductDto>>;

public record ProductDto(
    long Id,
    string Name,
    string? Description,
    long? CategoryId,
    string? CategoryName
);
