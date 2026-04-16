using MediatR;

namespace Application.Features.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(
    long Id,
    string Name,
    long? ParentId
) : IRequest;
