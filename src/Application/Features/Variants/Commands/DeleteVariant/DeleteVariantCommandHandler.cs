using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Variants.Commands.DeleteVariant;

public class DeleteVariantCommandHandler : IRequestHandler<DeleteVariantCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteVariantCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteVariantCommand request, CancellationToken cancellationToken)
    {
        var variant = await _unitOfWork.Variants.GetByIdAsync(request.Id);
        if (variant is null || variant.IsDeleted)
            throw new NotFoundException("Variant", request.Id);

        variant.IsDeleted = true;
        variant.DeletedAt = DateTime.UtcNow;

        _unitOfWork.Variants.Update(variant);
        await _unitOfWork.SaveChangesAsync();
    }
}
