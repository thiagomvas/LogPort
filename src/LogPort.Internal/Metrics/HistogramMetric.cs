namespace LogPort.Internal.Metrics;

public sealed class HistogramMetric
{
    private readonly RollingWindow<HistogramBucket> _window;
    private readonly double[] _boundaries;

    public HistogramMetric(TimeSpan bucketDuration, TimeSpan maxWindow, double[] boundaries)
    {
        _boundaries = boundaries;
        _window = new RollingWindow<HistogramBucket>(
            bucketDuration,
            maxWindow,
            () => new HistogramBucket(boundaries));
    }

    public void Observe(double value)
    {
        _window.GetCurrentBucket().Observe(value);
    }

    public long[] Query() => Query(_window.MaxWindow);
    public long[] Query(TimeSpan window)
    {
        var merged = new long[_boundaries.Length + 1];

        _window.ForEachBucketInWindow(window, b =>
        {
            var snapshot = b.Snapshot();
            for (int i = 0; i < merged.Length; i++)
                merged[i] += snapshot[i];
        });

        return merged;
    }

    public double[] Boundaries => _boundaries;
}