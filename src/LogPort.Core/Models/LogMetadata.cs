namespace LogPort.Core.Models;

/// <summary>
/// Represents aggregated metadata and statistics derived from a collection of log entries.
/// Used to power filters, dashboards, and summary views.
/// </summary>
public class LogMetadata
{
    /// <summary>
    /// All distinct log levels present in the result set.
    /// </summary>
    public string[] LogLevels { get; set; } = [];

    /// <summary>
    /// All distinct runtime environments present in the result set.
    /// </summary>
    public string[] Environments { get; set; } = [];

    /// <summary>
    /// All distinct service names present in the result set.
    /// </summary>
    public string[] Services { get; set; } = [];

    /// <summary>
    /// All distinct hostnames present in the result set.
    /// </summary>
    public string[] Hostnames { get; set; } = [];

    /// <summary>
    /// Number of log entries grouped by severity level.
    /// </summary>
    public Dictionary<string, int> LogCountByLevel { get; set; } = new();

    /// <summary>
    /// Number of log entries grouped by service name.
    /// </summary>
    public Dictionary<string, int> LogCountByService { get; set; } = new();

    /// <summary>
    /// Number of log entries grouped by runtime environment.
    /// </summary>
    public Dictionary<string, int> LogCountByEnvironment { get; set; } = new();

    /// <summary>
    /// Number of log entries grouped by hostname.
    /// </summary>
    public Dictionary<string, int> LogCountByHostname { get; set; } = new();

    /// <summary>
    /// Total number of log entries in the result set.
    /// </summary>
    public long LogCount { get; set; }
}