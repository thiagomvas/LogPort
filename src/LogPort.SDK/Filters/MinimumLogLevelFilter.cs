using LogPort.Core.Models;

namespace LogPort.SDK.Filters;

/// <summary>
/// Log level filter that allows only log entries at or above
/// a specified minimum severity level.
/// </summary>
public sealed class MinimumLogLevelFilter : ILogLevelFilter
{
    private readonly int _min;

    public MinimumLogLevelFilter(string minLevel)
    {
        _min = LogLevelSeverity.Map[minLevel];
    }
    /// <inheritdoc />
    public bool ShouldSend(LogEntry entry)
        => LogLevelSeverity.Map[entry.Level] >= _min;
}