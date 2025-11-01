using LogPort.Core.Models;
using Microsoft.AspNetCore.WebSockets;
using System.Text;
using System.Text.Json;
using LogPort.Core;
using LogPort.Internal.Common.Interface;
using Microsoft.Extensions.Logging;

namespace LogPort.Agent.Endpoints;

public static class LogEndpoints
{
    public static void MapLogEndpoints(this WebApplication app)
    {
        app.MapPost("/logs", AddLogAsync);

        app.MapGet("/logs", GetLogsAsync);

        MapStreamEndpoint(app);
    }

    private static void MapStreamEndpoint(WebApplication app)
    {
        app.Map("/stream", async context =>
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

            while (webSocket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), context.RequestAborted);
                if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Closing", context.RequestAborted);
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
                        logQueue.Enqueue(logEntry);
                        logger.LogInformation("Enqueued log: {LogEntry}", jsonMessage);
                    }
                }
                catch (JsonException)
                {
                    logger.LogWarning("Invalid JSON received: {Message}", jsonMessage);
                }
            }
        });
    }

    // Add a new log
    private static async Task<IResult> AddLogAsync(ILogRepository logRepository, LogEntry log)
    {
        await logRepository.AddLogAsync(log);
        return Results.Created($"/logs", log);
    }

    private static async Task<IResult> GetLogsAsync(
        ILogRepository logRepository,
        [AsParameters] LogQueryParameters parameters)
    {
        var logs = await logRepository.GetLogsAsync(parameters);
        return Results.Ok(logs);
    }
}
