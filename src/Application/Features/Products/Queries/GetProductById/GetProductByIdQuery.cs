using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(long Id) : IRequest<ProductDetailDto>;

public record ProductDetailDto(
    long Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    string? Sku,
    long? CategoryId,
    string? CategoryName
);
