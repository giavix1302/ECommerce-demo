using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Variants.Commands.CreateVariant;

public class CreateVariantCommandHandler : IRequestHandler<CreateVariantCommand, CreateVariantResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateVariantCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateVariantResult> Handle(CreateVariantCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        if (product is null || product.IsDeleted)
            throw new NotFoundException("Product", request.ProductId);

        if (request.Sku is not null && await _unitOfWork.Variants.ExistsBySkuAsync(request.Sku))
            throw new ConflictException($"SKU '{request.Sku}' already exists.");

        var variant = new ProductVariant
        {
            ProductId = request.ProductId,
            Sku = request.Sku,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            IsDefault = request.IsDefault,
            AttributeValues = request.Attributes.Select(a => new VariantAttributeValue
            {
                AttributeId = a.AttributeId,
                Value = a.Value
            }).ToList()
        };

        await _unitOfWork.Variants.AddAsync(variant);
        await _unitOfWork.SaveChangesAsync();

        return new CreateVariantResult(variant.Id, variant.ProductId, variant.Sku, variant.Price, variant.StockQuantity, variant.IsDefault);
    }
}
