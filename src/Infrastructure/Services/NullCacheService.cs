using Application.Common.Interfaces;

namespace Infrastructure.Services;

// Fallback khi không có Redis — bypass cache, luôn gọi factory
public class NullCacheService : ICacheService
{
    public Task<T?> GetOrCreateAsync<T>(string key, string prefix, Func<Task<T>> factory, TimeSpan ttl, CancellationToken ct = default)
        => factory()!;

    public Task RemoveAsync(string key, CancellationToken ct = default) => Task.CompletedTask;
    public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default) => Task.CompletedTask;
}
