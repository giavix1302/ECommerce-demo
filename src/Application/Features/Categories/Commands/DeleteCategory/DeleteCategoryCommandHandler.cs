using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Categories.Queries.GetCategories;
using MediatR;

namespace Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public DeleteCategoryCommandHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
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

        await _cache.RemoveByPrefixAsync(GetCategoriesQueryHandler.CachePrefix, cancellationToken);
    }
}
