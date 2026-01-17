using LogPort.Core.Models;

namespace LogPort.SDK.Filters;

/// <summary>
/// Log level filter that blocks log entries whose levels are explicitly blacklisted.
/// </summary>
public sealed class BlacklistLogLevelFilter : ILogLevelFilter
{
    private readonly HashSet<string> _blocked;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlacklistLogLevelFilter"/> class.
    /// </summary>
    /// <param name="levels">
    /// The log levels to block. Comparison is case-insensitive.
    /// </param>
    public BlacklistLogLevelFilter(params IEnumerable<string> levels)
    {
        _blocked = new HashSet<string>(levels, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public bool ShouldSend(LogEntry entry)
        => !_blocked.Contains(entry.Level);
}