namespace LogPort.Core.Models;

/// <summary>
/// Represents a single log entry with structured data
/// </summary>
public class LogEntry
{
    /// <summary>
    /// The human-readable log message.
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// The exact date and time when the log entry was created.
    /// </summary>
    /// <remarks>
    /// In most scenarios this would fallback to <see cref="DateTime.UtfNow"/>
    /// </remarks>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// The severity level of the log entry (e.g. Info, Warning, Error).
    /// </summary>
    /// <remarks>
    /// This property is normalized along the processing pipeline by the Agent using <see cref="LogNormalizer"/>
    /// </remarks>
    public string Level { get; set; } = "Info";

    /// <summary>
    /// The name of the service or application that produced the log entry.
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Additional structured metadata associated with the log entry.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = [];

    /// <summary>
    /// The distributed trace identifier associated with this log entry.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// The span identifier within the distributed trace.
    /// </summary>
    public string? SpanId { get; set; }

    /// <summary>
    /// The hostname of the machine where the log entry was generated.
    /// </summary>
    public string? Hostname { get; set; }

    /// <summary>
    /// The runtime environment in which the log entry was generated
    /// (e.g. Development, Staging, Production).
    /// </summary>
    public string? Environment { get; set; }
}