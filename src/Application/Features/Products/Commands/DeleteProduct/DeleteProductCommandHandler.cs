using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Products.Queries.GetProductById;
using Application.Features.Products.Queries.GetProducts;
using MediatR;

namespace Application.Features.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public DeleteProductCommandHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id);
        if (product is null || product.IsDeleted)
            throw new NotFoundException("Product", request.Id);

        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        await Task.WhenAll(
            _cache.RemoveByPrefixAsync(GetProductsQueryHandler.CachePrefix, cancellationToken),
            _cache.RemoveAsync($"{GetProductByIdQueryHandler.CachePrefix}:{request.Id}", cancellationToken)
        );
    }
}
