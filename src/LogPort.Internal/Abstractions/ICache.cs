namespace LogPort.Internal.Abstractions;

public interface ICache
{
    Task<T?> GetAsync<T>(string key);

    Task<T?> GetOrDefaultAsync<T>(string key, T? fallback = default);

    Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiration = null);

    Task<bool> ExistsAsync(string key);

    Task<bool> RemoveAsync(string key);
}