namespace LogPort.Core.Models;

public sealed class LogPattern
{
    public long Id { get; set; }
    public string NormalizedMessage { get; set; } = null!;
    public ulong PatternHash { get; set; } 
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public long OccurrenceCount { get; set; }
}
