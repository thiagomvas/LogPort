using LogPort.Core.Models;

namespace LogPort.Core.Interface;

public interface ILogRepository
{
    Task AddLogAsync(LogEntry log);
    Task<IEnumerable<LogEntry>> GetLogsAsync(LogQueryParameters parameters);
    Task<long> CountLogsAsync(LogQueryParameters parameters);
}