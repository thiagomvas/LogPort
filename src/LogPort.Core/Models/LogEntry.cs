namespace LogPort.Core.Models;

public class LogEntry
{
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
    public string Level { get; set; }
    public string ServiceName { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    
    public string? TraceId { get; set; }
    public string? SpanId { get; set; }
    public string? Hostname { get; set; }
    public string? Environment { get; set; }
}