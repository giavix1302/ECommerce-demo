using Application.Common.DTOs;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Variants.Queries.GetVariants;

public class GetVariantsQueryHandler : IRequestHandler<GetVariantsQuery, IEnumerable<VariantDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetVariantsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<VariantDto>> Handle(GetVariantsQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        if (product is null || product.IsDeleted)
            throw new NotFoundException("Product", request.ProductId);

        var variants = await _unitOfWork.Variants.GetByProductIdAsync(request.ProductId);

        return variants.Select(v => new VariantDto(
            v.Id,
            v.ProductId,
            v.Sku,
            v.Price,
            v.StockQuantity,
            v.IsDefault,
            v.AttributeValues.Select(av => new VariantAttributeDto(
                av.AttributeId,
                av.Attribute.Name,
                av.Value))
        ));
    }
}
