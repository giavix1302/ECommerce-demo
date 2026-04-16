using MediatR;

namespace Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Name,
    string? Description,
    long? CategoryId,
    IEnumerable<CreateProductVariantDto> Variants
) : IRequest<CreateProductResult>;

public record CreateProductVariantDto(
    string? Sku,
    decimal Price,
    int StockQuantity,
    bool IsDefault,
    IEnumerable<CreateProductAttributeDto> Attributes
);

public record CreateProductAttributeDto(
    long AttributeId,
    string Value
);

public record CreateProductResult(long Id, string Name);
