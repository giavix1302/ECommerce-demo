using MediatR;

namespace Application.Features.Variants.Commands.CreateVariant;

public record CreateVariantCommand(
    long ProductId,
    string? Sku,
    decimal Price,
    int StockQuantity,
    bool IsDefault,
    float? WeightKg,
    float? LengthCm,
    float? WidthCm,
    float? HeightCm,
    IEnumerable<CreateVariantAttributeDto> Attributes
) : IRequest<CreateVariantResult>;

public record CreateVariantAttributeDto(long AttributeId, string Value);

public record CreateVariantResult(long Id, long ProductId, string? Sku, decimal Price, int StockQuantity, bool IsDefault);
