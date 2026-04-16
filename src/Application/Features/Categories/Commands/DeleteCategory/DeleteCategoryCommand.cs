using MediatR;

namespace Application.Features.Categories.Commands.DeleteCategory;

public record DeleteCategoryCommand(long Id) : IRequest;
