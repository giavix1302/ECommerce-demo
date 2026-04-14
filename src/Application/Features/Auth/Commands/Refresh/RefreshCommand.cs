using MediatR;

namespace Application.Features.Auth.Commands.Refresh;

public record RefreshCommand(string RefreshToken) : IRequest<RefreshResult>;

public record RefreshResult(string AccessToken, string RefreshToken);
