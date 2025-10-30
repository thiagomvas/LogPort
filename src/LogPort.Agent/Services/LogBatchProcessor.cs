using LogPort.Core;
using LogPort.Core.Models;
using LogPort.Internal.Common.Interface;

namespace LogPort.Agent.Services;

public class LogBatchProcessor : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<LogBatchProcessor> _logger;
    private readonly LogQueue _queue;

    private readonly int _batchSize = 100;

    public LogBatchProcessor(IServiceProvider services, LogQueue queue, ILogger<LogBatchProcessor> logger)
    {
        _services = services;
        _queue = queue;
        _logger = logger;
        
        var config = services.GetRequiredService<LogPortConfig>();
        _batchSize = config.BatchSize > 0 ? config.BatchSize : _batchSize;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var batch = _queue.DequeueBatch(_batchSize).ToList();
            if (batch.Count == 0) continue;

            try
            {
                using var scope = _services.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<ILogRepository>();
                await repo.AddLogsAsync(batch);
                _logger.LogInformation("Inserted {Count} logs", batch.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting batch logs");
            }
        }
    }
}