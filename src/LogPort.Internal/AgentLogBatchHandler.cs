using LogPort.Core.Models;
using LogPort.Internal.Abstractions;
using LogPort.Internal.Metrics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LogPort.Internal;

public class AgentLogBatchHandler : ILogBatchHandler
{
    private readonly IServiceProvider _services;
    private readonly ILogger<AgentLogBatchHandler> _logger;
    private readonly MetricStore _metrics;

    public AgentLogBatchHandler(
        IServiceProvider services, 
        ILogger<AgentLogBatchHandler> logger,
        MetricStore metrics)
    {
        _services = services;
        _logger = logger;
        _metrics = metrics;
        
        _metrics.GetOrRegisterCounter("logs.processed.1h", 
            bucketDuration: TimeSpan.FromMinutes(1), 
            maxWindow: TimeSpan.FromMinutes(60));
        _metrics.GetOrRegisterCounter("logs.processed.24h",
            bucketDuration: TimeSpan.FromHours(1),
            maxWindow: TimeSpan.FromHours(24));
    }

    public async Task HandleBatchAsync(IEnumerable<LogEntry> batch, CancellationToken ct)
    {
        var levelCounts = new Dictionary<string, ulong>(StringComparer.OrdinalIgnoreCase);
        ulong totalCount = 0;

        using var scope = _services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ILogRepository>();
        var logEntries = batch.ToList();
        await repo.AddLogsAsync(logEntries); 


        foreach (var log in logEntries)
        {
            totalCount++;

            var level = log.Level.ToUpperInvariant();
            if (!levelCounts.TryGetValue(level, out var current))
                current = 0;
            levelCounts[level] = current + 1;
        }
        
        _metrics.Increment(Constants.Metrics.LogsProcessed, totalCount);
        _metrics.Increment("logs.processed.1h", totalCount);
        _metrics.Increment("logs.processed.24h", totalCount);

        foreach (var kvp in levelCounts)
        {
            var key = $"logs.processed.{kvp.Key}";
            _metrics.Increment(key, kvp.Value);
        }
        
        _logger.LogDebug("Inserted {Count} logs", totalCount);
    }
}