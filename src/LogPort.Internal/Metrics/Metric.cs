namespace LogPort.Internal.Metrics;

public class Metric
{
    private readonly TimeSpan _bucketDuration;
    private readonly int _bucketCount;

    private readonly Bucket[] _buckets;
    private readonly long[] _bucketKeys;

    public Metric(TimeSpan bucketDuration, TimeSpan maxWindow)
    {
        _bucketDuration = bucketDuration;
        _bucketCount = (int)(maxWindow.Ticks / bucketDuration.Ticks);

        _buckets = new Bucket[_bucketCount];
        _bucketKeys = new long[_bucketCount];

        for (int i = 0; i < _bucketCount; i++)
            _buckets[i] = new Bucket();
    }

    public void Increment(ulong value = 1)
    {
        GetCurrentBucket().Increment(value);
    }

    public ulong QueryCount(TimeSpan window)
    {
        var cutoff = GetBucketKey(DateTime.UtcNow - window);
        ulong total = 0;

        for (int i = 0; i < _bucketCount; i++)
        {
            if (Volatile.Read(ref _bucketKeys[i]) >= cutoff)
                total += Volatile.Read(ref _buckets[i].Counter);
        }

        return total;
    }
    
    private Bucket GetCurrentBucket()
    {
        var key = GetBucketKey(DateTime.UtcNow);
        var index = (int)(key % _bucketCount);

        if (Volatile.Read(ref _bucketKeys[index]) != key)
        {
            lock (_buckets[index])
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

    private long GetBucketKey(DateTime utcTime)
        => utcTime.Ticks / _bucketDuration.Ticks;
    
}