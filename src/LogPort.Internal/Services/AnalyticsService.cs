using LogPort.Core.Models;
using LogPort.Internal.Abstractions;
using LogPort.Internal.Models;

namespace LogPort.Internal.Services;

public class AnalyticsService
{
    private readonly ILogRepository _logRepository;

    public AnalyticsService(ILogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task<IReadOnlyList<LogBucket>> GetLogHistogramAsync(LogQueryParameters parameters,
        TimeSpan? stepDuration = null)
    {
        stepDuration ??= TimeSpan.FromHours(1);

        var buckets = new Dictionary<DateTime, ulong>();
        var current = parameters.From ?? DateTime.UtcNow.AddDays(-1);
        parameters.From ??= current;
        var to = parameters.To ?? DateTime.UtcNow;
        parameters.To ??= to;

        // Prefill
        while (current < to)
        {
            buckets[current] = 0;
            current = current.Add(stepDuration.Value);
        }

        await foreach (var batch in _logRepository.GetBatchesAsync(parameters, 1000))
        {
            foreach (var log in batch)
            {
                if (log.Timestamp < (parameters.From ?? DateTime.MinValue) ||
                    log.Timestamp > (parameters.To ?? DateTime.MaxValue))
                    continue;

                var bucketStart = AlignToStep(log.Timestamp, parameters.From.Value, stepDuration.Value);
                if (buckets.ContainsKey(bucketStart))
                    buckets[bucketStart]++;
            }
        }

        return buckets
            .OrderBy(b => b.Key)
            .Select(b => new LogBucket(b.Key, b.Value))
            .ToList();
    }

    public async Task<IReadOnlyDictionary<string, ulong>> GetCountByTypeAsync(LogQueryParameters parameters)
    {
        var counts = new Dictionary<string, ulong>(StringComparer.OrdinalIgnoreCase);

        await foreach (var batch in _logRepository.GetBatchesAsync(parameters, 1000))
        {
            foreach (var log in batch)
            {
                if (log.Timestamp < (parameters.From ?? DateTime.MinValue) ||
                    log.Timestamp > (parameters.To ?? DateTime.MaxValue))
                    continue;

                if (!counts.ContainsKey(log.Level))
                    counts[log.Level] = 0;

                counts[log.Level]++;
            }
        }

        return counts;

    }


    private static DateTime AlignToStep(DateTime timestamp, DateTime start, TimeSpan step)
    {
        var offset = timestamp - start;
        var steps = (long)(offset.Ticks / step.Ticks);
        return start.AddTicks(steps * step.Ticks);
    }
}