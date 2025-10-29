using LogPort.Core.Models;

namespace LogPort.Internal.Common.Interface;

public interface ILogRepository
{
    Task AddLogAsync(LogEntry log);
    Task AddLogsAsync(IEnumerable<LogEntry> logs);
    Task<IEnumerable<LogEntry>> GetLogsAsync(LogQueryParameters parameters);
    Task<long> CountLogsAsync(LogQueryParameters parameters);
}