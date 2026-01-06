using LogPort.Internal.Abstractions;

namespace LogPort.Internal.Metrics;

internal sealed class HistogramBucket : IRollingBucket
{
    private readonly long[] _counts;
    private readonly double[] _boundaries;

    public HistogramBucket(double[] boundaries)
    {
        _boundaries = boundaries;
        _counts = new long[boundaries.Length + 1]; 
    }

    public void Observe(double value)
    {
        int index = Array.FindIndex(_boundaries, b => value <= b);
        if (index == -1) index = _counts.Length - 1; 
        Interlocked.Increment(ref _counts[index]);
    }

    public long[] Snapshot()
    {
        var snapshot = new long[_counts.Length];
        for (int i = 0; i < _counts.Length; i++)
            snapshot[i] = Volatile.Read(ref _counts[i]);
        return snapshot;
    }

    public void Reset()
    {
        for (int i = 0; i < _counts.Length; i++)
            Interlocked.Exchange(ref _counts[i], 0);
    }

    public double[] Boundaries => _boundaries;
}
