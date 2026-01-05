using System.Collections.Concurrent;

namespace LogPort.Internal.Metrics;

public sealed class MetricStore
{
    private readonly ConcurrentDictionary<string, CounterMetric> _counters =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly TimeSpan _bucketDuration;
    private readonly TimeSpan _maxWindow;

    public MetricStore(TimeSpan bucketDuration, TimeSpan maxWindow)
    {
        _bucketDuration = bucketDuration;
        _maxWindow = maxWindow;
    }

    public CounterMetric GetOrRegisterCounter(string name)
        => _counters.GetOrAdd(name,
            _ => new CounterMetric(_bucketDuration, _maxWindow));


    public void Increment(string name, ulong value = 1)
        => GetOrRegisterCounter(name).Increment(value);

    public ulong QueryCount(string name, TimeSpan window)
        => GetOrRegisterCounter(name).Query(window);
}