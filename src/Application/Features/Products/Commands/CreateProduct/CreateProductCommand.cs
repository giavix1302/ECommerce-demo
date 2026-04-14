using MediatR;

namespace Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    string? Sku,
    long? CategoryId
) : IRequest<CreateProductResult>;

public record CreateProductResult(long Id, string Name, decimal Price);
