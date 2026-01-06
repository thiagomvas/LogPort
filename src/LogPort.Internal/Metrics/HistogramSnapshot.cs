namespace LogPort.Internal.Metrics;

public readonly struct HistogramSnapshot
{
    public long[] Counts { get; }
    public double[] Boundaries { get; }

    public HistogramSnapshot(long[] counts, double[] boundaries)
    {
        Counts = counts;
        Boundaries = boundaries;
    }
}