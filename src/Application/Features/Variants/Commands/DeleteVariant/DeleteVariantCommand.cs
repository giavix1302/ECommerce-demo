using MediatR;

namespace Application.Features.Variants.Commands.DeleteVariant;

public record DeleteVariantCommand(long Id) : IRequest;
