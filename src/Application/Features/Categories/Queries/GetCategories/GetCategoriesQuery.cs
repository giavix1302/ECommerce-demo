using MediatR;

namespace Application.Features.Categories.Queries.GetCategories;

public record GetCategoriesQuery : IRequest<IEnumerable<CategoryDto>>;

public record CategoryDto(
    long Id,
    string Name,
    string Slug,
    long? ParentId,
    string? ParentName,
    IEnumerable<CategoryChildDto> Children
);

public record CategoryChildDto(long Id, string Name, string Slug);
