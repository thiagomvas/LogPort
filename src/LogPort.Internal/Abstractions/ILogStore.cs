using LogPort.Core.Models;

namespace LogPort.Internal.Abstractions;

/// <summary>
/// Defines read and write operations for log storage.
/// </summary>
/// <remarks>
/// This abstraction represents the primary persistence layer for log entries.
/// Implementations are responsible for durability, filtering, paging, and batching.
/// </remarks>
public interface ILogStore
{
    /// <summary>
    /// Persists a single log entry.
    /// </summary>
    /// <param name="log">The log entry to persist.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task AddAsync(
        LogEntry log,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists multiple log entries in a single operation.
    /// </summary>
    /// <param name="logs">The log entries to persist.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task AddBatchAsync(
        IReadOnlyCollection<LogEntry> logs,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves logs matching the specified query parameters.
    /// </summary>
    /// <param name="query">Query parameters used to filter logs.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A read-only list of matching log entries.</returns>
    Task<IReadOnlyList<LogEntry>> GetAsync(
        LogQueryParameters query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams logs in ordered batches matching the specified query parameters.
    /// </summary>
    /// <param name="query">Query parameters used to filter logs.</param>
    /// <param name="batchSize">The maximum number of logs per batch.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An async stream of log entry batches.</returns>
    IAsyncEnumerable<IReadOnlyList<LogEntry>> GetBatchesAsync(
        LogQueryParameters query,
        int batchSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts logs matching the specified query parameters.
    /// </summary>
    /// <param name="query">Query parameters used to filter logs.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of matching log entries.</returns>
    Task<long> CountAsync(
        LogQueryParameters query,
        CancellationToken cancellationToken = default);
}
    