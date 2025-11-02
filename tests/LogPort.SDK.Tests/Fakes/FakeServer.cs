using LogPort.Core.Models;

namespace LogPort.SDK.Tests.Fakes;

public class FakeServer
{
    public bool IsOnline { get; set; } = true;
    public List<LogEntry> ReceivedLogs { get; } = new List<LogEntry>();
    
    public void ReceiveLog(LogEntry logEntry)
    {
        if (!IsOnline)
            return;
        ReceivedLogs.Add(logEntry);
    }
}