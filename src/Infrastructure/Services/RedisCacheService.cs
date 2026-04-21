using System.Text.Json;
using Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _redis;

    // Suffix để phân biệt tracking set với data key
    private static string TrackingSetKey(string prefix) => $"cache_keys:{prefix}";

    public RedisCacheService(IDistributedCache cache, IConnectionMultiplexer redis)
    {
        _cache = cache;
        _redis = redis;
    }

    public async Task<T?> GetOrCreateAsync<T>(string key, string prefix, Func<Task<T>> factory, TimeSpan ttl, CancellationToken ct = default)
    {
        var cached = await _cache.GetStringAsync(key, ct);
        if (cached is not null)
            return JsonSerializer.Deserialize<T>(cached);

        var value = await factory();
        if (value is null)
            return default;

        var json = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        }, ct);

        // Track key trong prefix set để có thể xóa theo prefix sau này
        var db = _redis.GetDatabase();
        await db.SetAddAsync(TrackingSetKey(prefix), key);
        await db.KeyExpireAsync(TrackingSetKey(prefix), ttl + TimeSpan.FromMinutes(1));

        return value;
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await _cache.RemoveAsync(key, ct);
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var trackingKey = TrackingSetKey(prefix);

        var members = await db.SetMembersAsync(trackingKey);
        if (members.Length == 0)
            return;

        var tasks = members.Select(m => _cache.RemoveAsync(m.ToString(), ct));
        await Task.WhenAll(tasks);
        await db.KeyDeleteAsync(trackingKey);
    }
}
