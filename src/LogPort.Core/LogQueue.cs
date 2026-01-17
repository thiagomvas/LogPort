using System.Collections.Concurrent;

using LogPort.Core.Models;

namespace LogPort.Core;

/// <summary>
/// Thread-safe queue for buffering log entries before processing, batching, or persistence.
/// </summary>
public class LogQueue
{
    private readonly ConcurrentQueue<LogEntry> _queue = new();

    /// <summary>
    /// Enqueues a log entry for later processing.
    /// </summary>
    /// <param name="log">The log entry to enqueue.</param>
    public void Enqueue(LogEntry log) => _queue.Enqueue(log);

    /// <summary>
    /// Attempts to dequeue a single log entry.
    /// </summary>
    /// <param name="log">
    /// When this method returns, contains the dequeued log entry
    /// if the operation succeeded.
    /// </param>
    /// <returns>
    /// <c>true</c> if a log entry was dequeued; otherwise, <c>false</c>.
    /// </returns>
    public bool TryDequeue(out LogEntry log) => _queue.TryDequeue(out log!);

    /// <summary>
    /// Gets the current number of log entries in the queue.
    /// </summary>
    public int Count => _queue.Count;

    /// <summary>
    /// Dequeues up to a specified number of log entries as a batch.
    /// </summary>
    /// <param name="maxBatchSize">The maximum number of log entries to dequeue.</param>
    /// <returns>
    /// A collection containing up to <paramref name="maxBatchSize"/> log entries.
    /// </returns>
    public IEnumerable<LogEntry> DequeueBatch(int maxBatchSize)
    {
        var batch = new List<LogEntry>();

        while (batch.Count < maxBatchSize && _queue.TryDequeue(out var log))
        {
            batch.Add(log);
        }

        return batch;
    }
}