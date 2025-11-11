using LogPort.Core.Models;

namespace LogPort.Internal.Common.Interface;

public interface ILogRepository
{
    Task AddLogAsync(LogEntry log);
    Task AddLogsAsync(IEnumerable<LogEntry> logs);
    Task<IEnumerable<LogEntry>> GetLogsAsync(LogQueryParameters parameters);
    IAsyncEnumerable<IReadOnlyList<LogEntry>> GetBatchesAsync(LogQueryParameters parameters, int batchSize);
    Task<long> CountLogsAsync(LogQueryParameters parameters);
    Task<LogMetadata> GetLogMetadataAsync();
}