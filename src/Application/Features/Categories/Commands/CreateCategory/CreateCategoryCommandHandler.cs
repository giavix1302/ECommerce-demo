using Application.Common.Exceptions;
using Application.Common.Helpers;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateCategoryResult> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var slug = SlugHelper.ToSlug(request.Name);
        if (await _unitOfWork.Categories.ExistsBySlugAsync(slug))
            throw new ConflictException($"Slug '{slug}' already exists.");

        if (request.ParentId.HasValue)
        {
            var parent = await _unitOfWork.Categories.GetByIdAsync(request.ParentId.Value);
            if (parent is null)
                throw new NotFoundException("Category", request.ParentId.Value);
        }

        var category = new Category
        {
            Name = request.Name,
            Slug = slug,
            ParentId = request.ParentId
        };

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return new CreateCategoryResult(category.Id, category.Name, category.Slug);
    }
}
