using LogPort.Core.Models;

namespace LogPort.Internal.Abstractions;

public interface ILogBatchHandler
{
    Task HandleBatchAsync(IEnumerable<LogEntry> batch, CancellationToken ct);

}