using LogPort.Core.Models;
using LogPort.Internal.Abstractions;

public class LogService
{
    private readonly ILogRepository _repository;
    
    public LogService(ILogRepository repository)
    {
        _repository = repository;
    }
    
    public async Task AddLogAsync(LogEntry log)
    {
        await _repository.AddLogAsync(log);
    }
    
    public async Task<IEnumerable<LogEntry>> GetLogsAsync(LogQueryParameters parameters)
    {
        return await _repository.GetLogsAsync(parameters);
    }
    
    public async Task<long> CountLogsAsync(LogQueryParameters parameters)
    {
        return await _repository.CountLogsAsync(parameters);
    }
    
    public async Task<LogMetadata> GetLogMetadataAsync()
    {
        return await _repository.GetLogMetadataAsync();
    }
}