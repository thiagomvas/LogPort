using LogPort.Core.Models;

namespace LogPort.Internal.Abstractions;

public interface ILogRepository
{
    Task AddLogAsync(LogEntry log);
    Task AddLogsAsync(IEnumerable<LogEntry> logs);
    Task<IEnumerable<LogEntry>> GetLogsAsync(LogQueryParameters parameters);
    Task<IEnumerable<LogEntry>> QueryLogsAsync(string query, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 100, CancellationToken cancellationToken = default);
    IAsyncEnumerable<IReadOnlyList<LogEntry>> GetBatchesAsync(LogQueryParameters parameters, int batchSize);
    Task<long> CountLogsAsync(LogQueryParameters parameters);

    Task<LogMetadata> GetLogMetadataAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? to = null);

    Task<LogPattern?> GetPatternByHashAsync(string patternHash);
    Task<long> CreatePatternAsync(string normalizedMessage, string patternHash, string level = "INFO");

    Task<long> GetOrCreatePatternAsync(
        string normalizedMessage,
        string patternHash,
        DateTime timestamp,
        string level = "INFO");

    Task UpdatePatternMessageAsync(long patternId, string normalizedMessage);

    Task<IReadOnlyList<LogPattern>> GetPatternsAsync(
        int limit = 100,
        int offset = 0);

    Task DeletePatternAsync(long patternId);
}