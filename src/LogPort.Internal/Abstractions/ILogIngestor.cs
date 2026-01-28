using LogPort.Core.Models;

namespace LogPort.Internal.Abstractions;

public interface ILogIngestor
{
    Task IngestAsync(IReadOnlyCollection<LogEntry> entries, CancellationToken cancellationToken = default);
}