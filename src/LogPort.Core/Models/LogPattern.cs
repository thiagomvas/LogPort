namespace LogPort.Core.Models;

public sealed class LogPattern
{
    public long Id { get; set; }
    public string NormalizedMessage { get; set; } = null!;
    public string PatternHash { get; set; } = null!;
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public long OccurrenceCount { get; set; }
}
