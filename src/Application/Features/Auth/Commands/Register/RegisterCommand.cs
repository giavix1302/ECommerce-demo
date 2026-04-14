using MediatR;

namespace Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FullName,
    string Phone
) : IRequest<RegisterResult>;

public record RegisterResult(long Id, string Email, string? FullName);
