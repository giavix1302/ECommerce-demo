using Application.Common.DTOs;
using Application.Features.Variants.Queries.GetVariants;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(long Id) : IRequest<ProductDetailDto>;

public record ProductDetailDto(
    long Id,
    string Name,
    string? Description,
    long? CategoryId,
    string? CategoryName,
    IEnumerable<VariantSummaryDto> Variants
);

public record VariantSummaryDto(
    long Id,
    string? Sku,
    decimal Price,
    int StockQuantity,
    bool IsDefault,
    IEnumerable<VariantAttributeDto> Attributes
);
