using LogPort.Core.Models;

namespace LogPort.Internal.Abstractions;

public interface ILogRepository
{
    Task AddLogAsync(LogEntry log);
    Task AddLogsAsync(IEnumerable<LogEntry> logs);
    Task<IEnumerable<LogEntry>> GetLogsAsync(LogQueryParameters parameters);
    IAsyncEnumerable<IReadOnlyList<LogEntry>> GetBatchesAsync(LogQueryParameters parameters, int batchSize);
    Task<long> CountLogsAsync(LogQueryParameters parameters);
    Task<LogMetadata> GetLogMetadataAsync();
    
    Task<LogPattern?> GetPatternByHashAsync(string patternHash);
    Task<long> CreatePatternAsync(string normalizedMessage, string patternHash);

    Task<long> GetOrCreatePatternAsync(
        string normalizedMessage,
        string patternHash,
        DateTime timestamp);

    Task UpdatePatternMessageAsync(long patternId, string normalizedMessage);

    Task<IReadOnlyList<LogPattern>> GetPatternsAsync(
        int limit = 100,
        int offset = 0);

    Task DeletePatternAsync(long patternId);
}