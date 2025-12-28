using LogPort.Internal.Abstractions;

using Microsoft.Extensions.Caching.Memory;

namespace LogPort.Agent.Services;

public class InMemoryCacheAdapter : ICache
{
    private readonly IMemoryCache _memoryCache;

    public InMemoryCacheAdapter(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        return Task.FromResult(_memoryCache.Get<T>(key));
    }

    public Task<T?> GetOrDefaultAsync<T>(string key, T? fallback = default)
    {
        return Task.FromResult(_memoryCache.Get<T>(key) ?? fallback);
    }

    public Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions();

        if (expiration.HasValue)
            options.SetAbsoluteExpiration(expiration.Value);

        _memoryCache.Set(key, value, options);

        return Task.FromResult(true);
    }

    public Task<bool> ExistsAsync(string key)
    {
        return Task.FromResult(_memoryCache.TryGetValue(key, out _));
    }

    public Task<bool> RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
        return Task.FromResult(true);
    }
}