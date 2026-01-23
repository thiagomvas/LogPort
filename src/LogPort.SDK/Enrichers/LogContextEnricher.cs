using LogPort.Core.Abstractions;
using LogPort.Core.Models;

namespace LogPort.SDK.Enrichers;

public sealed class LogContextEnricher : ILogEnricher
{
    public void Enrich(LogEntry entry)
    {
        if (LogContext.Current == null)
            return;

        foreach (var kv in LogContext.Current)
        {
            if (!entry.Metadata.ContainsKey(kv.Key))
                entry.Metadata[kv.Key] = kv.Value;
        }

    }
}