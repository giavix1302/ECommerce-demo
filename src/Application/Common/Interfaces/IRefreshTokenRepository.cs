using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task AddAsync(RefreshToken refreshToken);
    void Revoke(RefreshToken refreshToken);
    Task RevokeAllByUserIdAsync(long userId);
}
