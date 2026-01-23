using LogPort.Core.Models;

namespace LogPort.Core.Abstractions;

public interface ILogEnricher
{
    void Enrich(LogEntry entry);
}