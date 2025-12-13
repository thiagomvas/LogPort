using System.Text;
using System.Text.Json;
using LogPort.Core;
using LogPort.Core.Models;
using LogPort.Internal.Abstractions;
using Microsoft.Extensions.Logging;
using WebSocketManager = LogPort.Internal.Common.Services.WebSocketManager;

namespace LogPort.Agent.Endpoints;

public static class SocketEndpoints
{
    public static void MapSocketEndpoints(this WebApplication app)
    {
        MapStream(app);
        MapLiveLogs(app);
    }

    private static void MapStream(WebApplication app)
    {
        app.Map("api/stream", async context =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }

            using var socket = await context.WebSockets.AcceptWebSocketAsync();
            
            var buffer = new byte[8192];
            var queue = context.RequestServices.GetRequiredService<LogQueue>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            var normalizer = context.RequestServices.GetRequiredService<LogNormalizer>();
            var manager = context.RequestServices.GetRequiredService<WebSocketManager>();
            manager.AddSocket(socket);

            while (!context.RequestAborted.IsCancellationRequested && socket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer, context.RequestAborted);
                if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Closing",
                        context.RequestAborted);
                    break;
                }

                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                if (json == "ping")
                    continue;

                try
                {
                    var entry = JsonSerializer.Deserialize<LogEntry>(json);
                    if (entry != null)
                    {
                        entry.Level = normalizer.NormalizeLevel(entry.Level);
                        queue.Enqueue(entry);
                    }
                }
                catch (JsonException)
                {
                    logger.LogWarning("Invalid JSON received: {Message}", json);
                }
            }
            manager.RemoveSocket(socket);

        });
    }

    private static void MapLiveLogs(WebApplication app)
    {
        app.Map("api/live-logs", async context =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }

            using var socket = await context.WebSockets.AcceptWebSocketAsync();
            var manager = context.RequestServices.GetRequiredService<WebSocketManager>();
            manager.AddSocket(socket);

            var buffer = new byte[4096];

            while (!context.RequestAborted.IsCancellationRequested && socket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer, context.RequestAborted);
                if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Closing",
                        context.RequestAborted);
                    break;
                }
            }

            manager.RemoveSocket(socket);
        });
    }
}
