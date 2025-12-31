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
        var result = await _repository.GetLogsAsync(parameters);

        return result;
    }

    public async Task<long> CountLogsAsync(LogQueryParameters parameters)
    {
        var key = $"{CacheKeys.CountPrefix}{parameters.GetCacheKey()}";

        var cachedCount = await _cache.GetAsync<long?>(key);
        if (cachedCount is not null)
        {
            _logger?.LogDebug("Getting log count from cache with key: {CacheKey}", key);
            return cachedCount.Value;
        }

        var result = await _repository.CountLogsAsync(parameters);

        await _cache.SetAsync(key, result, _config.Cache.DefaultExpiration);

        return result;
    }

    public async Task<LogMetadata> GetLogMetadataAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? to = null)
    {
        var key = CacheKeys.BuildLogMetadataCacheKey(from, to);

        var cachedMetadata = await _cache.GetAsync<LogMetadata>(key);
        if (cachedMetadata is not null)
        {
            _logger?.LogDebug(
                "Getting log metadata from cache with key: {CacheKey}",
                key
            );
            return cachedMetadata;
        }

        var result = await _repository.GetLogMetadataAsync(from, to);

        await _cache.SetAsync(
            key,
            result,
            _config.Cache.DefaultExpiration
        );

        return result;
    }


    public async Task<IEnumerable<LogPattern>> GetLogPatternsAsync(int limit = 100, int offset = 0)
    {
        var key = CacheKeys.LogPatterns;

        var cachedPatterns = await _cache.GetAsync<IEnumerable<LogPattern>>(key);
        if (cachedPatterns is not null)
        {
            _logger?.LogDebug("Getting log patterns from cache with key: {CacheKey}", key);
            return cachedPatterns;
        }

        var result = await _repository.GetPatternsAsync(limit, offset);

        await _cache.SetAsync(key, result, _config.Cache.DefaultExpiration);

        return result;
    }


}