using LogPort.Internal.Abstractions;

namespace LogPort.Internal.Metrics;


internal sealed class RollingWindow<TBucket>
    where TBucket : IRollingBucket
{
    private readonly TimeSpan _bucketDuration;
    private readonly int _bucketCount;

    private readonly TBucket[] _buckets;
    private readonly long[] _bucketKeys;

    public RollingWindow(
        TimeSpan bucketDuration,
        TimeSpan maxWindow,
        Func<TBucket> bucketFactory)
    {
        if (bucketDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(bucketDuration));

        if (maxWindow < bucketDuration)
            throw new ArgumentOutOfRangeException(nameof(maxWindow));

        _bucketDuration = bucketDuration;
        _bucketCount = (int)(maxWindow.Ticks / bucketDuration.Ticks);

        if (_bucketCount <= 0)
            throw new InvalidOperationException("Rolling window must have at least one bucket");

        _buckets = new TBucket[_bucketCount];
        _bucketKeys = new long[_bucketCount];

        for (int i = 0; i < _bucketCount; i++)
            _buckets[i] = bucketFactory();
    }

    public TBucket GetCurrentBucket()
    {
        var key = GetBucketKey(DateTime.UtcNow);
        var index = (int)(key % _bucketCount);

        if (Volatile.Read(ref _bucketKeys[index]) != key)
        {
            lock (_buckets[index]!)
            {
                if (_bucketKeys[index] != key)
                {
                    _bucketKeys[index] = key;
                    _buckets[index].Reset();
                }
            }
        }

        return _buckets[index];
    }

    public void ForEachBucketInWindow(
        TimeSpan window,
        Action<TBucket> action)
    {
        var cutoff = GetBucketKey(DateTime.UtcNow - window);

        for (int i = 0; i < _bucketCount; i++)
        {
            if (Volatile.Read(ref _bucketKeys[i]) >= cutoff)
                action(_buckets[i]);
        }
    }

    private long GetBucketKey(DateTime utcTime)
        => utcTime.Ticks / _bucketDuration.Ticks;
}