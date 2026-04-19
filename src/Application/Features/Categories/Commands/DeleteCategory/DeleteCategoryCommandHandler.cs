using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Id);
        if (category is null)
            throw new NotFoundException("Category", request.Id);


        var hasChildren = await _unitOfWork.Categories.HasChildCategoriesAsync(request.Id);
        if (hasChildren)
            throw new BadRequestException("Cannot delete a category that has child categories. Please delete or reassign child categories first.");

        _unitOfWork.Categories.Delete(category);
        await _unitOfWork.SaveChangesAsync();
    }
}
