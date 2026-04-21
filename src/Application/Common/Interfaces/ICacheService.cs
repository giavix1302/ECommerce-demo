namespace Application.Common.Interfaces;

public interface ICacheService
{
    Task<T?> GetOrCreateAsync<T>(string key, string prefix, Func<Task<T>> factory, TimeSpan ttl, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);
}
