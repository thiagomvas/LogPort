using LogPort.Core.Models;
using LogPort.SDK.Filters;

namespace LogPort.SDK;

public sealed class DisableAllLogsFilter : ILogLevelFilter
{
    public bool ShouldSend(LogEntry _) => false;
}
