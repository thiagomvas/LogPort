using LogPort.Core.Models;
using Microsoft.AspNetCore.WebSockets;
using System.Text;
using System.Text.Json;
using LogPort.Core;
using LogPort.Internal.Abstractions;
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
    }

// Add a new log
    private static async Task<IResult> AddLogAsync(LogService logRepository, LogEntry log)
    {
        await logRepository.AddLogAsync(log);
        return Results.Created($"api/logs", log);
    }

    private static async Task<IResult> GetLogsAsync(
        LogService logRepository,
        [AsParameters] LogQueryParameters parameters)
    {
        var logs = await logRepository.GetLogsAsync(parameters);
        return Results.Ok(logs);
    }
    private static async Task<IResult> CountLogsAsync(
        LogService logRepository,
        [AsParameters] LogQueryParameters parameters)
    {
        var count = await logRepository.CountLogsAsync(parameters);
        return Results.Ok(new { Count = count });
    }

    private static async Task<IResult> GetLogMetadataAsync(LogService repository)
    {
        var metadata = await repository.GetLogMetadataAsync();
        return Results.Ok(metadata);
    }
}
