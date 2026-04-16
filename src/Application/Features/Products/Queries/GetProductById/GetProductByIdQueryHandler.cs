using Application.Common.DTOs;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductDetailDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdWithVariantsAsync(request.Id);

        if (product is null || product.IsDeleted)
            throw new NotFoundException("Product", request.Id);

        return new ProductDetailDto(
            product.Id,
            product.Name,
            product.Description,
            product.CategoryId,
            product.Category?.Name,
            product.Variants.Where(v => !v.IsDeleted).Select(v => new VariantSummaryDto(
                v.Id,
                v.Sku,
                v.Price,
                v.StockQuantity,
                v.IsDefault,
                v.AttributeValues.Select(va => new VariantAttributeDto(
                    va.AttributeId,
                    va.Attribute.Name,
                    va.Value
                ))
            ))
            );
    }
}
