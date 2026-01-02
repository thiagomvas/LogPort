using LogPort.Core.Models;

namespace LogPort.SDK;

public interface ILogLevelFilter
{
    bool ShouldSend(LogEntry entry);
}