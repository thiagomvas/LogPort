namespace LogPort.Internal.Metrics;

public sealed class CounterMetric
{
    private readonly RollingWindow<CounterBucket> _window;

    public CounterMetric(TimeSpan bucketDuration, TimeSpan maxWindow)
    {
        _window = new RollingWindow<CounterBucket>(
            bucketDuration,
            maxWindow,
            () => new CounterBucket());
    }

    public void Increment(ulong value = 1)
    {
        _window.GetCurrentBucket().Increment(value);
    }

    public ulong Query(TimeSpan window)
    {
        ulong total = 0;

        _window.ForEachBucketInWindow(window, b =>
        {
            total += Volatile.Read(ref b.Value);
        });

        return total;
    }
}
