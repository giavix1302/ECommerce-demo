using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken);
        if (token is null || token.IsRevoked)
            return;

        _unitOfWork.RefreshTokens.Revoke(token);
        await _unitOfWork.SaveChangesAsync();
    }
}
