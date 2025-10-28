namespace LogPort.Core.Models;

public class LogEntry
{
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
    public string Level { get; set; }
}