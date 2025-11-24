namespace LogPort.Internal.Abstractions;

public interface ICache
{
    Task<T?> GetAsync<T>(string key);
    Task<bool> TryGetAsync<T>(string key, out T? value);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
}