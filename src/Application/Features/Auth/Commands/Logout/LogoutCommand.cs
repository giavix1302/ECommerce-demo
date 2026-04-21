using MediatR;

namespace Application.Features.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken, string? Jti, DateTime? AccessTokenExpiry) : IRequest;
