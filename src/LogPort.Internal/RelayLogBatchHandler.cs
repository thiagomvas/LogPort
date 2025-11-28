using LogPort.Core.Models;
using LogPort.Internal.Abstractions;
using LogPort.SDK;

namespace LogPort.Internal;

public class RelayLogBatchHandler : ILogBatchHandler
{
    private readonly LogPortClient _client;
    
    public RelayLogBatchHandler(LogPortClient client)
    {
        _client = client;
    }
    
    public async Task HandleBatchAsync(IEnumerable<LogEntry> batch, CancellationToken ct)
    {
        await _client.EnsureConnectedAsync(ct);
        _client.LogBatch(batch);
        await _client.FlushAsync();
    }
}