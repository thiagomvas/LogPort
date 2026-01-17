namespace LogPort.Core.Models;

/// <summary>
/// Represents a normalized log pattern aggregated from multiple similar log entries.
/// Used for grouping, counting occurrences, and tracking lifecycle information.
/// </summary>
public sealed class LogPattern
{
    /// <summary>
    /// Unique identifier of the log pattern.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// The normalized form of the log message with variable parts removed or replaced.
    /// </summary>
    public string NormalizedMessage { get; set; } = null!;

    /// <summary>
    /// Hash value computed from the normalized message, used for fast comparisons and grouping.
    /// </summary>
    public ulong PatternHash { get; set; }

    /// <summary>
    /// Timestamp of the first occurrence of this log pattern.
    /// </summary>
    public DateTime FirstSeen { get; set; }

    /// <summary>
    /// Timestamp of the most recent occurrence of this log pattern.
    /// </summary>
    public DateTime LastSeen { get; set; }

    /// <summary>
    /// Total number of times this log pattern has occurred.
    /// </summary>
    public long OccurrenceCount { get; set; }

    /// <summary>
    /// The severity level associated with this log pattern
    /// </summary>
    public string Level { get; set; } = null!;
}