using System.Threading.RateLimiting;
using StackExchange.Redis;

namespace API.RateLimit;

/// <summary>
/// FixedWindow rate limiter backed by Redis.
/// Dùng INCR + EXPIRE — atomic, không cần Lua script phức tạp.
/// </summary>
public sealed class RedisFixedWindowRateLimiter : RateLimiter
{
    private readonly string _key;
    private readonly int _limit;
    private readonly int _windowSeconds;
    private readonly IDatabase _db;

    public RedisFixedWindowRateLimiter(
        string key,
        int limit,
        int windowSeconds,
        IConnectionMultiplexer redis)
    {
        _key = key;
        _limit = limit;
        _windowSeconds = windowSeconds;
        _db = redis.GetDatabase();
    }

    public override TimeSpan? IdleDuration => null;

    public override RateLimiterStatistics? GetStatistics() => null;

    protected override ValueTask<RateLimitLease> AcquireAsyncCore(
        int permitCount,
        CancellationToken cancellationToken)
        => new(AcquireInternalAsync(permitCount));

    protected override RateLimitLease AttemptAcquireCore(int permitCount)
    {
        // Synchronous path — chạy async trên thread pool để không deadlock
        return AcquireInternalAsync(permitCount).GetAwaiter().GetResult();
    }

    private async Task<RateLimitLease> AcquireInternalAsync(int permitCount)
    {
        // INCR trả về giá trị sau khi tăng
        var count = await _db.StringIncrementAsync(_key, permitCount);

        // Chỉ set TTL ở lần đầu tiên (count == permitCount)
        // tránh reset window mỗi request
        if (count == permitCount)
            await _db.KeyExpireAsync(_key, TimeSpan.FromSeconds(_windowSeconds));

        if (count <= _limit)
            return new RedisRateLimitLease(isAcquired: true);

        return new RedisRateLimitLease(isAcquired: false);
    }

    protected override void Dispose(bool disposing) { }
}

internal sealed class RedisRateLimitLease : RateLimitLease
{
    private readonly bool _acquired;

    public RedisRateLimitLease(bool isAcquired) => _acquired = isAcquired;

    public override bool IsAcquired => _acquired;

    public override IEnumerable<string> MetadataNames => [];

    public override bool TryGetMetadata(string metadataName, out object? metadata)
    {
        metadata = null;
        return false;
    }

    protected override void Dispose(bool disposing) { }
}
