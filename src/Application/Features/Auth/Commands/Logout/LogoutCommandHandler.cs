using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenBlacklistService _blacklist;

    public LogoutCommandHandler(IUnitOfWork unitOfWork, ITokenBlacklistService blacklist)
    {
        _unitOfWork = unitOfWork;
        _blacklist = blacklist;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken);
        if (token is null || token.IsRevoked)
            return;

        _unitOfWork.RefreshTokens.Revoke(token);
        await _unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrEmpty(request.Jti) && request.AccessTokenExpiry.HasValue)
        {
            var ttl = request.AccessTokenExpiry.Value - DateTime.UtcNow;
            if (ttl > TimeSpan.Zero)
                await _blacklist.BlacklistAsync(request.Jti, ttl, cancellationToken);
        }
    }
}
