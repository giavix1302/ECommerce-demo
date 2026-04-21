using Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Services;

public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly IDistributedCache _cache;

    public TokenBlacklistService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task BlacklistAsync(string jti, TimeSpan ttl, CancellationToken ct = default)
    {
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl };
        await _cache.SetStringAsync($"blacklist:{jti}", "1", options, ct);
    }

    public async Task<bool> IsBlacklistedAsync(string jti, CancellationToken ct = default)
    {
        var value = await _cache.GetStringAsync($"blacklist:{jti}", ct);
        return value is not null;
    }
}
