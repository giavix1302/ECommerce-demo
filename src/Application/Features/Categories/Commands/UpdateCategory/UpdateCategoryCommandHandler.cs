using Application.Common.Exceptions;
using Application.Common.Helpers;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Id);
        if (category is null)
            throw new NotFoundException("Category", request.Id);

        var slug = SlugHelper.ToSlug(request.Name);
        if (slug != category.Slug && await _unitOfWork.Categories.ExistsBySlugAsync(slug))
            throw new ConflictException($"Slug '{slug}' already exists.");

        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == request.Id)
                throw new ConflictException("A category cannot be its own parent.");

            var parent = await _unitOfWork.Categories.GetByIdAsync(request.ParentId.Value);
            if (parent is null)
                throw new NotFoundException("Category", request.ParentId.Value);
        }

        category.Name = request.Name;
        category.Slug = slug;
        category.ParentId = request.ParentId;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();
    }
}
