using MediatR;

namespace Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<LoginInternalResult>;

public record LoginResult(string AccessToken, string Email, string? FullName, string Role);

public record LoginInternalResult(string AccessToken, string RefreshToken, string Email, string? FullName, string Role);
