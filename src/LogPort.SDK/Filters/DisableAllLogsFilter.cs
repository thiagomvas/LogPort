using LogPort.Core.Models;
using LogPort.SDK.Filters;

namespace LogPort.SDK;
/// <summary>
/// Log Filter that blocks all log entries.
/// </summary>
public sealed class DisableAllLogsFilter : ILogLevelFilter
{
    /// <inheritdoc />
    public bool ShouldSend(LogEntry _) => false;
}