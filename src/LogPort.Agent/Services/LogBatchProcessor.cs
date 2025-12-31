using LogPort.Core;
using LogPort.Core.Models;
using LogPort.Internal;
using LogPort.Internal.Abstractions;
using LogPort.Internal.Configuration;

using WebSocketManager = LogPort.Internal.Common.Services.WebSocketManager;

namespace LogPort.Agent.Services;

public class LogBatchProcessor : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<LogBatchProcessor> _logger;
    private readonly LogQueue _queue;
    private readonly WebSocketManager _socketManager;

    private readonly int _batchSize = 100;
    private readonly TimeSpan _flushInterval = TimeSpan.FromSeconds(1);

    public LogBatchProcessor(IServiceProvider services, LogQueue queue, WebSocketManager socketManager, ILogger<LogBatchProcessor> logger)
    {
        _services = services;
        _queue = queue;
        _logger = logger;
        _socketManager = socketManager;

        var config = services.GetRequiredService<LogPortConfig>();
        _batchSize = config.BatchSize > 0 ? config.BatchSize : _batchSize;
        _flushInterval = config.FlushIntervalMs > 0 ? TimeSpan.FromMilliseconds(config.FlushIntervalMs) : _flushInterval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_flushInterval, stoppingToken);
            var batch = _queue.DequeueBatch(_batchSize).ToList();
            if (batch.Count == 0) continue;

            try
            {
                await _socketManager.BroadcastBatchAsync(batch);

                using var scope = _services.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<ILogBatchHandler>();
                await handler.HandleBatchAsync(batch, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting batch logs");
            }
        }
    }
}