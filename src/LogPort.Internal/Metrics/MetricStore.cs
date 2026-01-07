using System.Collections.Concurrent;

using LogPort.Internal.Configuration;

namespace LogPort.Internal.Metrics;

public sealed class MetricStore
{
    private readonly ConcurrentDictionary<string, CounterMetric> _counters =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly ConcurrentDictionary<string, HistogramMetric> _histograms =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly TimeSpan _bucketDuration;
    private readonly TimeSpan _maxWindow;
    private readonly double[] _defaultHistogramBoundaries = new double[] { 1, 5, 10, 25, 50, 100, 250, 500, 1000 };

    public MetricStore(TimeSpan bucketDuration, TimeSpan maxWindow)
    {
        _bucketDuration = bucketDuration;
        _maxWindow = maxWindow;
    }

    public MetricStore(LogPortConfig config)
        : this(config.Metrics.BucketDuration, config.Metrics.MaxWindow)
    {
    }

    public CounterMetric GetOrRegisterCounter(string name)
        => _counters.GetOrAdd(name,
            _ => new CounterMetric(_bucketDuration, _maxWindow));

    public CounterMetric GetOrRegisterCounter(string name, TimeSpan bucketDuration, TimeSpan maxWindow)
        => _counters.GetOrAdd(name, _ => new CounterMetric(bucketDuration, maxWindow));


    public void Increment(string name, ulong value = 1)
        => GetOrRegisterCounter(name).Increment(value);

    public ulong QueryCount(string name, TimeSpan window)
        => GetOrRegisterCounter(name).Query(window);

    public HistogramMetric GetOrRegisterHistogram(string name, double[]? boundaries = null)
        => _histograms.GetOrAdd(name,
            _ => new HistogramMetric(
                _bucketDuration,
                _maxWindow,
                boundaries ?? _defaultHistogramBoundaries));

    public HistogramMetric GetOrRegisterHistogram(string name, TimeSpan bucketDuration, TimeSpan maxWindow,
        double[]? boundaries = null)
        => _histograms.GetOrAdd(name, _ => new HistogramMetric(bucketDuration, maxWindow, boundaries ?? _defaultHistogramBoundaries));

    public void Observe(string name, double value, double[]? boundaries = null)
        => GetOrRegisterHistogram(name, boundaries).Observe(value);

    public long[] QueryHistogram(string name, TimeSpan window)
        => GetOrRegisterHistogram(name).Query(window);

    public MetricsSnapshot Snapshot()
    {
        var now = DateTime.UtcNow;

        var counters = new Dictionary<string, CounterSnapshot>(
            _counters.Count,
            StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in _counters)
        {
            var metric = kvp.Value;

            if (kvp.Key.EndsWith(".24h", StringComparison.OrdinalIgnoreCase))
            {
                counters[kvp.Key] = new CounterSnapshot(metric.QueryBuckets(24));
            }
            else if (kvp.Key.EndsWith(".1h", StringComparison.OrdinalIgnoreCase))
            {
                counters[kvp.Key] = new CounterSnapshot(metric.QueryBuckets(60));
            }
            else
            {
                counters[kvp.Key] = new CounterSnapshot(
                    last1s: metric.Query(TimeSpan.FromSeconds(1)),
                    last10s: metric.Query(TimeSpan.FromSeconds(10)),
                    last1m: metric.Query(TimeSpan.FromMinutes(1))
                );
            }
        }

        var histograms = new Dictionary<string, HistogramSnapshot>(
            _histograms.Count,
            StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in _histograms)
        {
            var metric = kvp.Value;

            histograms[kvp.Key] = new HistogramSnapshot(
                counts: metric.Query(TimeSpan.FromMinutes(1)), // adjust window as needed
                boundaries: metric.Boundaries);
        }


        return new MetricsSnapshot(now, counters, histograms);
    }
}