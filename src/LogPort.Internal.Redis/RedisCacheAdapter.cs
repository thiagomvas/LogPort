using System.Text.Json;

using LogPort.Internal.Abstractions;

using StackExchange.Redis;

namespace LogPort.Internal.Redis;

public class RedisCacheAdapter : ICache
{
    private readonly IDatabase _db;
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RedisCacheAdapter(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty) return default;

        return JsonSerializer.Deserialize<T>(value!, Options);
    }

    public async Task<T?> GetOrDefaultAsync<T>(string key, T? fallback = default)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty) return fallback;

        return JsonSerializer.Deserialize<T>(value!, Options);
    }

    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var json = JsonSerializer.Serialize(value, Options);
        return await _db.StringSetAsync(
            key,
            json,
            expiry: expiration,
            when: When.Always,
            flags: CommandFlags.None
        );

    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await _db.KeyExistsAsync(key);
    }

    public async Task<bool> RemoveAsync(string key)
    {
        return await _db.KeyDeleteAsync(key);
    }
}