using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Variants.Commands.UpdateVariant;

public class UpdateVariantCommandHandler : IRequestHandler<UpdateVariantCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateVariantCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateVariantCommand request, CancellationToken cancellationToken)
    {
        var variant = await _unitOfWork.Variants.GetByIdAsync(request.Id);
        if (variant is null || variant.IsDeleted)
            throw new NotFoundException("Variant", request.Id);

        if (request.Sku is not null && request.Sku != variant.Sku
            && await _unitOfWork.Variants.ExistsBySkuAsync(request.Sku, excludeId: request.Id))
            throw new ConflictException($"SKU '{request.Sku}' already exists.");

        variant.Sku = request.Sku;
        variant.Price = request.Price;
        variant.StockQuantity = request.StockQuantity;
        variant.IsDefault = request.IsDefault;
        variant.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Variants.Update(variant);
        await _unitOfWork.SaveChangesAsync();
    }
}
