using LogPort.Core.Models;
using LogPort.Internal.Abstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LogPort.Internal;

public class AgentLogBatchHandler : ILogBatchHandler
{
    private readonly IServiceProvider _services;
    private readonly ILogger<AgentLogBatchHandler> _logger;

    public AgentLogBatchHandler(IServiceProvider services, ILogger<AgentLogBatchHandler> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task HandleBatchAsync(IEnumerable<LogEntry> batch, CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ILogRepository>();
        await repo.AddLogsAsync(batch);
        _logger.LogDebug("Inserted {Count} logs", batch.Count());
    }
}