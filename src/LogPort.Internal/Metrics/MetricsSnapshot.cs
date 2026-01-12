namespace LogPort.Internal.Metrics;

public sealed class MetricsSnapshot
{
    public DateTime TimestampUtc { get; set; }
    public IReadOnlyDictionary<string, CounterSnapshot> Counters { get; }
    public IReadOnlyDictionary<string, HistogramSnapshot> Histograms { get; }

    public MetricsSnapshot(
        DateTime timestampUtc,
        IReadOnlyDictionary<string, CounterSnapshot> counters, IReadOnlyDictionary<string, HistogramSnapshot> histograms)
    {
        TimestampUtc = timestampUtc;
        Counters = counters;
        Histograms = histograms;
    }
}