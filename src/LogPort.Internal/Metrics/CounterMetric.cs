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
    
    public ulong[] QueryBuckets(int count)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        var buckets = new ulong[count];
        var nowKey = DateTime.UtcNow.Ticks / _window.BucketDuration.Ticks;

        _window.ForEachBucketInWindow(_window.BucketDuration * count, (bucket, key) =>
        {
            int index = (int)(key - (nowKey - count + 1)); 
            if (index >= 0 && index < count)
                buckets[index] = Volatile.Read(ref bucket.Value);
        });

        return buckets;
    }


}
