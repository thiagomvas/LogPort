using LogPort.Core.Models;

namespace LogPort.SDK.Filters;

public sealed class BlacklistLogLevelFilter : ILogLevelFilter
{
    private readonly HashSet<string> _blocked;

    public BlacklistLogLevelFilter(params IEnumerable<string> levels)
    {
        _blocked = new HashSet<string>(levels, StringComparer.OrdinalIgnoreCase);
    }

    public bool ShouldSend(LogEntry entry)
        => !_blocked.Contains(entry.Level);
}