using MediatR;

namespace Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<LoginResult>;

public record LoginResult(string AccessToken, string RefreshToken, string Email, string? FullName, string Role);
