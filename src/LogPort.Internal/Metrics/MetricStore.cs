using System.Collections.Concurrent;

namespace LogPort.Internal.Metrics;

public sealed class MetricStore
{
    private readonly ConcurrentDictionary<string, Metric> _metrics = new ConcurrentDictionary<string, Metric>(StringComparer.OrdinalIgnoreCase);

    private readonly TimeSpan _bucketDuration;
    private readonly TimeSpan _maxWindow;

    public MetricStore(TimeSpan bucketDuration, TimeSpan maxWindow)
    {
        if (bucketDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(bucketDuration));

        if (maxWindow < bucketDuration)
            throw new ArgumentOutOfRangeException(
                nameof(maxWindow),
                "maxWindow must be >= bucketDuration");

        _bucketDuration = bucketDuration;
        _maxWindow = maxWindow;
    }

    public Metric GetOrRegisterMetric(string name)
    {
        return _metrics.GetOrAdd(name, _ => new Metric(_bucketDuration, _maxWindow));
    }

    public void Increment(string name, ulong value = 1)
    {
        var metric = GetOrRegisterMetric(name);
        metric.Increment(value);
    }

    public ulong QueryCount(string name, TimeSpan window)
    {
        return GetOrRegisterMetric(name)
            .QueryCount(window);
    }
    
    public IReadOnlyCollection<string> Names => (IReadOnlyCollection<string>)_metrics.Keys;
}