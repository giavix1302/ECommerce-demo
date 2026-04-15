using MediatR;

namespace Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    long Id,
    string Name,
    string? Description,
    long? CategoryId
) : IRequest;
