using LogPort.Core.Models;

namespace LogPort.SDK.Filters;

/// <summary>
/// Log level filter that allows only log entries whose levels are explicitly whitelisted.
/// </summary>
public sealed class WhitelistLogLevelFilter : ILogLevelFilter
{
    private readonly HashSet<string> _allowed;

    public WhitelistLogLevelFilter(params IEnumerable<string> levels)
    {
        _allowed = new HashSet<string>(levels, StringComparer.OrdinalIgnoreCase);
    }
    /// <inheritdoc />
    public bool ShouldSend(LogEntry entry)
        => _allowed.Contains(entry.Level);
}