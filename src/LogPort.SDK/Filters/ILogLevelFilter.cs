using LogPort.Core.Models;

namespace LogPort.SDK.Filters;

/// <summary>
/// Defines a filter that determines whether a log entry should be sent.
/// </summary>
public interface ILogLevelFilter
{
    /// <summary>
    /// Determines whether the specified log entry should be sent.
    /// </summary>
    /// <param name="entry">The log entry to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the log entry should be sent; otherwise, <c>false</c>.
    /// </returns>
    bool ShouldSend(LogEntry entry);
}
