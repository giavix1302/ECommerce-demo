using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Products.Queries.GetProductById;
using Application.Features.Products.Queries.GetProducts;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public UpdateProductCommandHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id);
        if (product is null || product.IsDeleted)
            throw new NotFoundException("Product", request.Id);

        product.Name = request.Name;
        product.Description = request.Description;
        product.CategoryId = request.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        await Task.WhenAll(
            _cache.RemoveByPrefixAsync(GetProductsQueryHandler.CachePrefix, cancellationToken),
            _cache.RemoveAsync($"{GetProductByIdQueryHandler.CachePrefix}:{request.Id}", cancellationToken)
        );
    }
}
