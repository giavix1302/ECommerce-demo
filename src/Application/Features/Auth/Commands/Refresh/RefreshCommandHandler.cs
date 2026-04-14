using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands.Refresh;

public class RefreshCommandHandler : IRequestHandler<RefreshCommand, RefreshResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshCommandHandler(IUnitOfWork unitOfWork, IJwtTokenService jwtTokenService)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<RefreshResult> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken);

        if (existingToken is null || existingToken.IsRevoked || existingToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        // Revoke token cũ
        _unitOfWork.RefreshTokens.Revoke(existingToken);

        // Tạo cặp token mới
        var newAccessToken = _jwtTokenService.GenerateAccessToken(existingToken.User);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken(existingToken.UserId);

        await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken);
        await _unitOfWork.SaveChangesAsync();

        return new RefreshResult(newAccessToken, newRefreshToken.Token);
    }
}
