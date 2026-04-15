using MediatR;

namespace Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Name,
    string? Description,
    long? CategoryId
) : IRequest<CreateProductResult>;

public record CreateProductResult(long Id, string Name);
