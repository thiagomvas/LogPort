using LogPort.Core.Models;
using LogPort.Internal.Abstractions;

public class LogService
{
    private readonly ILogRepository _repository;
    private readonly ICache _cache;

    public LogService(ILogRepository repository, ICache cache)
    {
        _repository = repository;
        _cache = cache;
    }
    
    public async Task AddLogAsync(LogEntry log)
    {
        await _repository.AddLogAsync(log);
    }
    
    public async Task<IEnumerable<LogEntry>> GetLogsAsync(LogQueryParameters parameters)
    {
        if (await _cache.TryGetAsync<IEnumerable<LogEntry>>($"logs_{parameters.GetCacheKey()}", out var cachedLogs) 
            && cachedLogs is not null)
        {
            return cachedLogs;
        }
        
        return await _repository.GetLogsAsync(parameters);
    }
    
    public async Task<long> CountLogsAsync(LogQueryParameters parameters)
    {
        if (await _cache.TryGetAsync<long>($"count_{parameters.GetCacheKey()}", out var cachedCount))
        {
            return cachedCount;
        }
        return await _repository.CountLogsAsync(parameters);
    }
    
    public async Task<LogMetadata> GetLogMetadataAsync()
    {
        if (await _cache.TryGetAsync<LogMetadata>("metadata", out var cachedMetadata) 
            && cachedMetadata is not null)
        {
            return cachedMetadata;
        }
        return await _repository.GetLogMetadataAsync();
    }
}