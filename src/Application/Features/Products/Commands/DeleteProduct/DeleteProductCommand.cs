using MediatR;

namespace Application.Features.Products.Commands.DeleteProduct;

public record DeleteProductCommand(long Id) : IRequest;
