using System.Net.WebSockets;

using LogPort.Core.Models;

using Microsoft.Extensions.Logging;

namespace LogPort.Internal.Common.Services;

public class WebSocketManager
{
    private readonly HashSet<WebSocket> _sockets = new();
    private readonly Lock _lock = new();
    private readonly ILogger<WebSocketManager>? _logger;

    public WebSocketManager(ILogger<WebSocketManager>? logger = null)
    {
        _logger = logger;
        _logger?.LogInformation("WebSocketManager initialized");
    }

    public void AddSocket(WebSocket socket)
    {
        lock (_lock)
        {
            _sockets.Add(socket);
        }
        _logger?.LogInformation("WebSocket connected. Total connections: {Count}", _sockets.Count);
    }

    public void RemoveSocket(WebSocket socket)
    {
        lock (_lock)
        {
            _sockets.Remove(socket);
        }
        _logger?.LogInformation("WebSocket disconnected. Total connections: {Count}", _sockets.Count);
    }

    public IEnumerable<WebSocket> GetAllSockets()
    {
        lock (_lock)
        {
            return _sockets.ToList();
        }
    }

    public Task BroadcastAsync(LogEntry log)
    {
        return BroadcastBatchAsync(new[] { log });
    }

    public async Task BroadcastBatchAsync(IEnumerable<LogEntry> logs)
    {
        if (logs == null || !logs.Any())
            return;

        var message = System.Text.Json.JsonSerializer.Serialize(logs);
        var buffer = System.Text.Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(buffer);

        List<WebSocket> sockets;
        lock (_lock)
        {
            sockets = _sockets.ToList();
        }

        foreach (var socket in sockets)
        {
            if (socket.State == WebSocketState.Open)
            {
                try
                {
                    await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch
                {
                    RemoveSocket(socket);
                }
            }
        }
    }

    public void AbortAll()
    {
        List<WebSocket> sockets;
        lock (_lock)
        {
            sockets = _sockets.ToList();
            _sockets.Clear();
        }

        foreach (var socket in sockets)
        {
            try
            {
                if (socket.State == WebSocketState.Open ||
                    socket.State == WebSocketState.CloseReceived)
                {
                    socket.Abort();
                }
            }
            catch { }
        }

        _logger?.LogInformation("All WebSockets aborted");
    }


}