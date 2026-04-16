using MediatR;

namespace Application.Features.Variants.Commands.UpdateVariant;

public record UpdateVariantCommand(
    long Id,
    string? Sku,
    decimal Price,
    int StockQuantity,
    bool IsDefault
) : IRequest;
