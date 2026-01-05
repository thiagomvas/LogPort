namespace LogPort.Internal.Metrics;

public sealed class MetricsSnapshot
{
    public DateTime TimestampUtc  { get; set; }
    public IReadOnlyDictionary<string, CounterSnapshot> Counters { get;  }

    public MetricsSnapshot(
        DateTime timestampUtc,
        IReadOnlyDictionary<string, CounterSnapshot> counters)
    {
        TimestampUtc = timestampUtc;
        Counters = counters;
    }
}