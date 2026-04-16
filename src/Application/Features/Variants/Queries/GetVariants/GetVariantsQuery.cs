using Application.Common.DTOs;
using MediatR;

namespace Application.Features.Variants.Queries.GetVariants;

public record GetVariantsQuery(long ProductId) : IRequest<IEnumerable<VariantDto>>;

public record VariantDto(
    long Id,
    long ProductId,
    string? Sku,
    decimal Price,
    int StockQuantity,
    bool IsDefault,
    IEnumerable<VariantAttributeDto> Attributes
);


