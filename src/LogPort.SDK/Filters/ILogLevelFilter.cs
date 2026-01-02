using LogPort.Core.Models;

namespace LogPort.SDK.Filters;

public interface ILogLevelFilter
{
    bool ShouldSend(LogEntry entry);
}