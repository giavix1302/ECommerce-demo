using MediatR;

namespace Application.Features.Variants.Commands.UpdateVariant;

public record UpdateVariantCommand(
    long Id,
    string Sku,
    decimal Price,
    int StockQuantity,
    bool IsDefault,
    float? WeightKg,
    float? LengthCm,
    float? WidthCm,
    float? HeightCm,
    List<UpdateVariantAttributeDto>? Attributes
) : IRequest;

public record UpdateVariantAttributeDto(string AttributeName, string Value);
