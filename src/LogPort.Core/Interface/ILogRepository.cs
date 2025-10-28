using LogPort.Core.Models;

namespace LogPort.Core.Interface;

public interface ILogRepository
{
    Task AddLogAsync(LogEntry log);
    Task<IEnumerable<LogEntry>> GetLogsAsync(DateTime? from = null, DateTime? to = null, string? level = null);
}