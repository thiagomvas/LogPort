using LogPort.Core.Models;

namespace LogPort.SDK.Filters;

public sealed class MinimumLogLevelFilter : ILogLevelFilter
{
    private readonly int _min;

    public MinimumLogLevelFilter(string minLevel)
    {
        _min = LogLevelSeverity.Map[minLevel];
    }

    public bool ShouldSend(LogEntry entry)
        => LogLevelSeverity.Map[entry.Level] >= _min;
}