using LogPort.Core.Models;
using Microsoft.AspNetCore.WebSockets;
using System.Text;
using System.Text.Json;
using LogPort.Core;
using LogPort.Internal.Common.Interface;
using Microsoft.Extensions.Logging;
using WebSocketManager = LogPort.Internal.Common.Services.WebSocketManager;

namespace LogPort.Agent.Endpoints;

public static class LogEndpoints
{
    public static void MapLogEndpoints(this WebApplication app)
    {
        app.MapPost("api/logs", AddLogAsync);

        app.MapGet("api/logs", GetLogsAsync);
        app.MapGet("api/logs/count", CountLogsAsync);
        app.MapGet("api/logs/metadata", GetLogMetadataAsync);

        MapStreamEndpoint(app);
        MapLiveLogsEndpoint(app);
    }

    private static void MapStreamEndpoint(WebApplication app)
    {
        app.Map("api/stream", async context =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }

            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var buffer = new byte[8192];
            var logQueue = context.RequestServices.GetRequiredService<LogQueue>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            var normalizer = context.RequestServices.GetRequiredService<LogNormalizer>();

            while (webSocket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), context.RequestAborted);
                if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Closing",
                        context.RequestAborted);
                    break;
                }

                var jsonMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

                if (jsonMessage == "ping")
                    continue;

                try
                {
                    var logEntry = JsonSerializer.Deserialize<LogEntry>(jsonMessage);
                    if (logEntry != null)
                    {
                        logEntry.Level = normalizer.NormalizeLevel(logEntry.Level);
                        logQueue.Enqueue(logEntry);
                    }
                }
                catch (JsonException)
                {
                    logger.LogWarning("Invalid JSON received: {Message}", jsonMessage);
                }
            }
        });
    }

    private static void MapLiveLogsEndpoint(WebApplication app)
    {
        app.Map("api/live-logs", async context =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }

            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var webSocketManager = context.RequestServices.GetRequiredService<WebSocketManager>();
            webSocketManager.AddSocket(webSocket);

            var buffer = new byte[1024 * 4];
            while (webSocket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), context.RequestAborted);
                if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Closing",
                        context.RequestAborted);
                    break;
                }
            }

            webSocketManager.RemoveSocket(webSocket);
        });
        
    }

// Add a new log
    private static async Task<IResult> AddLogAsync(ILogRepository logRepository, LogEntry log)
    {
        await logRepository.AddLogAsync(log);
        return Results.Created($"api/logs", log);
    }

    private static async Task<IResult> GetLogsAsync(
        ILogRepository logRepository,
        [AsParameters] LogQueryParameters parameters)
    {
        var logs = await logRepository.GetLogsAsync(parameters);
        return Results.Ok(logs);
    }
    private static async Task<IResult> CountLogsAsync(
        ILogRepository logRepository,
        [AsParameters] LogQueryParameters parameters)
    {
        var count = await logRepository.CountLogsAsync(parameters);
        return Results.Ok(new { Count = count });
    }

    private static async Task<IResult> GetLogMetadataAsync(ILogRepository repository)
    {
        var metadata = await repository.GetLogMetadataAsync();
        return Results.Ok(metadata);
    }
}
