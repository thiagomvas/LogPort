using System.Collections.Concurrent;
using LogPort.Core.Models;

namespace LogPort.Core;

public class LogQueue
{
    private readonly ConcurrentQueue<LogEntry> _queue = new();
    public void Enqueue(LogEntry log) => _queue.Enqueue(log);
    public bool TryDequeue(out LogEntry log) => _queue.TryDequeue(out log);
    public int Count => _queue.Count;
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