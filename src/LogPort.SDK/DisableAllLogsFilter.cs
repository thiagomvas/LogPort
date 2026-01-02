using LogPort.Core.Models;

namespace LogPort.SDK;

public sealed class DisableAllLogsFilter : ILogLevelFilter
{
    public bool ShouldSend(LogEntry _) => false;
}
