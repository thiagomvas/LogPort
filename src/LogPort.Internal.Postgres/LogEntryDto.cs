using System.Text.Json;

using LogPort.Core.Models;

namespace LogPort.Data.Postgres;

public sealed class LogEntryDto
{
    public DateTime Timestamp { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Metadata { get; set; } = string.Empty; // JSON string
    public string TraceId { get; set; }
    public string SpanId { get; set; }
    public string Hostname { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    
    /// <summary>
    /// Maps the DTO to a LogEntry, deserializing the Metadata JSON string.
    /// </summary>
    public LogEntry ToLogEntry()
    {
        return new LogEntry
        {
            Timestamp = this.Timestamp,
            ServiceName = this.ServiceName,
            Level = this.Level,
            Message = this.Message,
            Metadata = string.IsNullOrEmpty(this.Metadata)
                ? new Dictionary<string, object>()
                : JsonSerializer.Deserialize<Dictionary<string, object>>(this.Metadata) ?? new(),
            TraceId = this.TraceId,
            SpanId = this.SpanId,
            Hostname = this.Hostname,
            Environment = this.Environment
        };
    }
}