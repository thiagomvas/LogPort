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

    public Task<bool> TryGetAsync<T>(string key, out T? value)
    {
        if (_memoryCache.TryGetValue(key, out var cachedValue) && cachedValue is T typedValue)
        {
            value = typedValue;
            return Task.FromResult(true);
        }

        value = default;
        return Task.FromResult(false);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.SetAbsoluteExpiration(expiration.Value);
        }
        _memoryCache.Set(key, value, options);
        return Task.CompletedTask;
    }
}