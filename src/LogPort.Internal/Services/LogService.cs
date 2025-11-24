using LogPort.Core.Models;
using LogPort.Internal;
using LogPort.Internal.Abstractions;
using Microsoft.Extensions.Logging;

public class LogService
{
    private readonly ILogRepository _repository;
    private readonly ICache _cache;
    private readonly LogPortConfig _config;
    private readonly ILogger<LogService> _logger;

    public LogService(ILogRepository repository, ICache cache, LogPortConfig config, ILogger<LogService>? logger = null)
    {
        _repository = repository;
        _cache = cache;
        _config = config;
        _logger = logger;
    }
    
    public async Task AddLogAsync(LogEntry log)
    {
        await _repository.AddLogAsync(log);
    }
    
    public async Task<IEnumerable<LogEntry>> GetLogsAsync(LogQueryParameters parameters)
    {
        if (await _cache.TryGetAsync<IEnumerable<LogEntry>>($"{CacheKeys.LogPrefix}{parameters.GetCacheKey()}", out var cachedLogs) 
            && cachedLogs is not null)
        {
            _logger?.LogDebug("Getting logs from cache with key: {CacheKey}", $"{CacheKeys.LogPrefix}{parameters.GetCacheKey()}");
            return cachedLogs;
        }
        var result = await _repository.GetLogsAsync(parameters);
        await _cache.SetAsync($"{CacheKeys.LogPrefix}{parameters.GetCacheKey()}", result, _config.Cache.DefaultExpiration);
        return result;
    }
    
    public async Task<long> CountLogsAsync(LogQueryParameters parameters)
    {
        if (await _cache.TryGetAsync<long>($"{CacheKeys.CountPrefix}{parameters.GetCacheKey()}", out var cachedCount))
        {
            _logger?.LogDebug("Getting log count from cache with key: {CacheKey}", $"{CacheKeys.CountPrefix}{parameters.GetCacheKey()}");
            return cachedCount;
        }
        var result = await _repository.CountLogsAsync(parameters);
        await _cache.SetAsync($"{CacheKeys.CountPrefix}{parameters.GetCacheKey()}", result, _config.Cache.DefaultExpiration);
        return result;
    }
    
    public async Task<LogMetadata> GetLogMetadataAsync()
    {
        if (await _cache.TryGetAsync<LogMetadata>(CacheKeys.LogMetadata, out var cachedMetadata) 
            && cachedMetadata is not null)
        {
            _logger?.LogDebug("Getting log metadata from cache with key: {CacheKey}", CacheKeys.LogMetadata);
            return cachedMetadata;
        }
        var result = await _repository.GetLogMetadataAsync();
        await _cache.SetAsync(CacheKeys.LogMetadata, result, _config.Cache.DefaultExpiration);
        return result;
    }
}