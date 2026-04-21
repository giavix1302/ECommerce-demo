using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Products.Queries.GetProductById;
using Application.Features.Products.Queries.GetProducts;
using MediatR;

namespace Application.Features.Variants.Commands.DeleteVariant;

public class DeleteVariantCommandHandler : IRequestHandler<DeleteVariantCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public DeleteVariantCommandHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task Handle(DeleteVariantCommand request, CancellationToken cancellationToken)
    {
        var variant = await _unitOfWork.Variants.GetByIdAsync(request.Id);
        if (variant is null || variant.IsDeleted)
            throw new NotFoundException("Variant", request.Id);

        var productId = variant.ProductId;
        variant.IsDeleted = true;
        variant.DeletedAt = DateTime.UtcNow;

        _unitOfWork.Variants.Update(variant);
        await _unitOfWork.SaveChangesAsync();

        await Task.WhenAll(
            _cache.RemoveByPrefixAsync(GetProductsQueryHandler.CachePrefix, cancellationToken),
            _cache.RemoveAsync($"{GetProductByIdQueryHandler.CachePrefix}:{productId}", cancellationToken)
        );
    }
}
