namespace Application.Common.Interfaces;

public interface ITokenBlacklistService
{
    Task BlacklistAsync(string jti, TimeSpan ttl, CancellationToken ct = default);
    Task<bool> IsBlacklistedAsync(string jti, CancellationToken ct = default);
}
