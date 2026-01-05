namespace LogPort.Internal.Configuration;

public class CacheConfig
{
    /// <summary>
    /// Gets or sets whether the cache should use redis or not.
    /// </summary>
    public bool UseRedis { get; set; } = false;

    /// <summary>
    /// Gets or sets the connection string for the redis cache.
    /// </summary>
    public string? RedisConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the default expiration of cached data.
    /// </summary>
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(10);
}