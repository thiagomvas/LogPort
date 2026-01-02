using LogPort.Core.Models;

namespace LogPort.SDK.Filters;

public sealed class WhitelistLogLevelFilter : ILogLevelFilter
{
    private readonly HashSet<string> _allowed;

    public WhitelistLogLevelFilter(IEnumerable<string> levels)
    {
        _allowed = new HashSet<string>(levels, StringComparer.OrdinalIgnoreCase);
    }

    public bool ShouldSend(LogEntry entry)
        => _allowed.Contains(entry.Level);
}
