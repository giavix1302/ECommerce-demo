using MediatR;

namespace Application.Features.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(
    string Name,
    long? ParentId
) : IRequest<CreateCategoryResult>;

public record CreateCategoryResult(long Id, string Name, string Slug);
