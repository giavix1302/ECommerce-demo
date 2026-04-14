using MediatR;

namespace Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    long Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    string? Sku,
    long? CategoryId
) : IRequest;
